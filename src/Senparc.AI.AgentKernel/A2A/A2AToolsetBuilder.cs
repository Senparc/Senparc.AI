using A2ASdk = global::A2A;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.A2A
{
    /// <summary>
    /// A2A 工具集构建器。
    /// </summary>
    public static class A2AToolsetBuilder
    {
        public const string DefaultPublicBaseUrlEnvName = "A2A_PUBLIC_BASE_URL";

        public const string DefaultSystemPrompt = "你是一个会主动调用 A2A 工具的助手。凡是可以通过工具完成的问题，优先调用工具并给出结果。";

        /// <summary>
        /// 根据配置准备 A2A 工具集合（用于 ChatOptions.Tools）。
        /// </summary>
        public static async Task<A2AToolsetResult> PrepareAsync(A2AAgentOption option, string? resolvedBaseUrl = null)
        {
            ArgumentNullException.ThrowIfNull(option);

            resolvedBaseUrl ??= ResolveBaseUrl(option);
            if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
            {
                throw new InvalidOperationException("未解析到可用的 A2A Base URL。");
            }

            if (!Uri.TryCreate(resolvedBaseUrl, UriKind.Absolute, out var baseUri))
            {
                throw new InvalidOperationException($"A2A Base URL 无效：{resolvedBaseUrl}");
            }

            var bindingMode = option.GetBindingMode();
            if (bindingMode != A2AToolBindingMode.LocalFunctionProxy)
            {
                throw new NotSupportedException($"当前版本暂仅支持 {A2AToolBindingMode.LocalFunctionProxy} 模式。");
            }

            var httpClient = CreateHttpClient(option.AuthorizationBearerToken);
            var cardResolver = new A2ASdk.A2ACardResolver(baseUri, httpClient, option.AgentCardPath, logger: null);

            A2ASdk.AgentCard? agentCard = null;
            string? cardDiscoveryError = null;

            try
            {
                agentCard = await cardResolver.GetAgentCardAsync();
            }
            catch (Exception ex)
            {
                cardDiscoveryError = ex.Message;
            }

            if (agentCard == null)
            {
                httpClient.Dispose();
                throw new InvalidOperationException($"A2A Agent Card 获取失败：{cardDiscoveryError ?? "未知错误"}");
            }

            A2ASdk.IA2AClient? a2aClient = null;
            try
            {
                var clientOptions = new A2ASdk.A2AClientOptions();
                if (option.PreferredBindings.Count > 0)
                {
                    clientOptions.PreferredBindings = [.. option.PreferredBindings];
                }

                a2aClient = A2ASdk.A2AClientFactory.Create(agentCard, httpClient, clientOptions);
                var chatTools = BuildChatTools(option, a2aClient, agentCard);

                return new A2AToolsetResult
                {
                    AgentOption = option,
                    BindingMode = bindingMode,
                    ResolvedBaseUrl = resolvedBaseUrl,
                    BaseUri = baseUri,
                    AgentCard = agentCard,
                    DiscoveredSkillNames = agentCard.Skills?.Select(s => s.Name ?? s.Id).Where(s => !string.IsNullOrWhiteSpace(s)).Cast<string>().ToList() ?? [],
                    ChatTools = chatTools,
                    CardDiscoveryError = cardDiscoveryError,
                    RuntimeA2AClient = a2aClient,
                    RuntimeHttpClient = httpClient
                };
            }
            catch
            {
                if (a2aClient is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                httpClient.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 生成 A2A 场景常用的 ChatClientAgentOptions。
        /// </summary>
        public static ChatClientAgentOptions CreateChatClientAgentOptions(
            IReadOnlyList<AITool> chatTools,
            string? systemPrompt = null,
            string agentName = "A2AAgent",
            string agentDescription = "An assistant that can call A2A tools.",
            float temperature = 0.2f,
            float topP = 0.2f,
            int maxOutputTokens = 2000)
        {
            ArgumentNullException.ThrowIfNull(chatTools);

            return new ChatClientAgentOptions
            {
                Name = agentName,
                Description = agentDescription,
                ChatOptions = new ChatOptions
                {
                    Instructions = string.IsNullOrWhiteSpace(systemPrompt)
                        ? DefaultSystemPrompt
                        : systemPrompt,
                    Temperature = temperature,
                    TopP = topP,
                    MaxOutputTokens = maxOutputTokens,
                    Tools = [.. chatTools],
                    AllowMultipleToolCalls = true
                }
            };
        }

        /// <summary>
        /// 通过 A2A 构建结果直接生成 ChatClientAgentOptions。
        /// </summary>
        public static ChatClientAgentOptions CreateChatClientAgentOptions(
            this A2AToolsetResult result,
            string? systemPrompt = null,
            string agentName = "A2AAgent",
            string agentDescription = "An assistant that can call A2A tools.",
            float temperature = 0.2f,
            float topP = 0.2f,
            int maxOutputTokens = 2000)
        {
            ArgumentNullException.ThrowIfNull(result);
            return CreateChatClientAgentOptions(result.ChatTools, systemPrompt, agentName, agentDescription, temperature, topP, maxOutputTokens);
        }

        /// <summary>
        /// 解析可用 Base URL：
        /// BaseUrl(公网) > LocalBaseUrl + PublicBaseUrl/环境变量映射 > LocalBaseUrl(原值)。
        /// </summary>
        public static string? ResolveBaseUrl(A2AAgentOption option, string publicBaseUrlEnvName = DefaultPublicBaseUrlEnvName)
        {
            ArgumentNullException.ThrowIfNull(option);

            if (!string.IsNullOrWhiteSpace(option.BaseUrl))
            {
                return option.BaseUrl;
            }

            if (string.IsNullOrWhiteSpace(option.LocalBaseUrl))
            {
                return null;
            }

            var publicBaseUrl = option.PublicBaseUrl;
            if (string.IsNullOrWhiteSpace(publicBaseUrl))
            {
                publicBaseUrl = Environment.GetEnvironmentVariable(publicBaseUrlEnvName);
            }

            if (string.IsNullOrWhiteSpace(publicBaseUrl))
            {
                return option.LocalBaseUrl;
            }

            if (!TryMergePublicBaseUrl(publicBaseUrl, option.LocalBaseUrl, out var mergedUrl, out _))
            {
                return option.LocalBaseUrl;
            }

            return mergedUrl;
        }

        /// <summary>
        /// 用公网 Base URL 替换本地地址的域名/端口，保留原 LocalBaseUrl 的路径和查询参数。
        /// </summary>
        public static bool TryMergePublicBaseUrl(string publicBaseUrl, string localBaseUrl, out string mergedUrl, out string error)
        {
            mergedUrl = localBaseUrl;
            error = string.Empty;

            if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var publicUri))
            {
                error = $"PublicBaseUrl 不是合法 URL：{publicBaseUrl}";
                return false;
            }

            if (!Uri.TryCreate(localBaseUrl, UriKind.Absolute, out var localUri))
            {
                error = $"LocalBaseUrl 不是合法 URL：{localBaseUrl}";
                return false;
            }

            var builder = new UriBuilder(publicUri)
            {
                Path = localUri.AbsolutePath,
                Query = localUri.Query.TrimStart('?')
            };
            mergedUrl = builder.Uri.ToString();

            return true;
        }

        /// <summary>
        /// 判断 URL 是否为本地回环地址。
        /// </summary>
        public static bool IsLocalAddress(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (uri.IsLoopback)
            {
                return true;
            }

            if (IPAddress.TryParse(uri.Host, out var ipAddress))
            {
                return IPAddress.IsLoopback(ipAddress);
            }

            return false;
        }

        private static HttpClient CreateHttpClient(string? bearerToken)
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            }

            return client;
        }

        private static IReadOnlyList<AITool> BuildChatTools(A2AAgentOption option, A2ASdk.IA2AClient client, A2ASdk.AgentCard card)
        {
            var tools = new List<AITool>();
            var prefix = SanitizeToolName(string.IsNullOrWhiteSpace(option.Name) ? option.AgentName ?? "a2a" : option.Name, "a2a");

            // 始终提供一个通用发送工具，保证没有 skills 时也能可调用。
            tools.Add(BuildSendTool(client, $"{prefix}_send_message", $"发送消息到 A2A Agent（{card.Name ?? option.Name}）。", null));

            var skills = card.Skills?.ToList() ?? new List<A2ASdk.AgentSkill>();
            if (option.AllowedSkills.Count > 0)
            {
                var whiteSet = option.AllowedSkills.ToHashSet(StringComparer.OrdinalIgnoreCase);
                skills = skills.Where(s => whiteSet.Contains(s.Id ?? string.Empty) || whiteSet.Contains(s.Name ?? string.Empty)).ToList();
            }

            foreach (var skill in skills)
            {
                var idOrName = skill.Id ?? skill.Name ?? "skill";
                var toolName = $"{prefix}_{SanitizeToolName(idOrName, "skill")}";
                var toolDescription = string.IsNullOrWhiteSpace(skill.Description)
                    ? $"调用 A2A Skill：{skill.Name ?? skill.Id}"
                    : $"{skill.Description}（A2A Skill：{skill.Name ?? skill.Id}）";

                tools.Add(BuildSendTool(client, toolName, toolDescription, skill));
            }

            return tools;
        }

        private static AIFunction BuildSendTool(A2ASdk.IA2AClient client, string toolName, string description, A2ASdk.AgentSkill? skill)
        {
            async Task<string> SendAsync(string input, CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return "请输入要发送给 A2A Agent 的内容。";
                }

                var payload = BuildPayload(input, skill);
                var response = await A2ASdk.A2AClientExtensions.SendMessageAsync(
                    client,
                    payload,
                    A2ASdk.Role.User,
                    string.Empty,
                    cancellationToken);
                return ExtractResponseText(response);
            }

            return AIFunctionFactory.Create((Delegate)SendAsync, new AIFunctionFactoryOptions
            {
                Name = toolName,
                Description = description
            });
        }

        private static string BuildPayload(string input, A2ASdk.AgentSkill? skill)
        {
            if (skill == null)
            {
                return input;
            }

            var name = string.IsNullOrWhiteSpace(skill.Name) ? skill.Id : skill.Name;
            return $"请优先使用技能「{name}」处理请求：\n{input}";
        }

        private static string ExtractResponseText(A2ASdk.SendMessageResponse response)
        {
            return response.PayloadCase switch
            {
                A2ASdk.SendMessageResponseCase.Message => ExtractMessageText(response.Message),
                A2ASdk.SendMessageResponseCase.Task => ExtractTaskText(response.Task),
                _ => "A2A 返回了未知响应类型。"
            };
        }

        private static string ExtractTaskText(A2ASdk.AgentTask? task)
        {
            if (task == null)
            {
                return "A2A 返回了空任务。";
            }

            var lines = new List<string>();
            var statusText = ExtractMessageText(task.Status?.Message);
            if (!string.IsNullOrWhiteSpace(statusText))
            {
                lines.Add(statusText);
            }

            if (task.History is { Count: > 0 })
            {
                foreach (var msg in task.History)
                {
                    var text = ExtractMessageText(msg);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        lines.Add(text);
                    }
                }
            }

            if (task.Artifacts is { Count: > 0 })
            {
                foreach (var artifact in task.Artifacts)
                {
                    if (artifact?.Parts is not { Count: > 0 })
                    {
                        continue;
                    }

                    var artifactText = string.Join("\n", artifact.Parts.Select(ExtractPartText).Where(s => !string.IsNullOrWhiteSpace(s)));
                    if (!string.IsNullOrWhiteSpace(artifactText))
                    {
                        lines.Add(artifactText);
                    }
                }
            }

            if (lines.Count == 0)
            {
                return $"A2A 任务状态：{task.Status?.State}";
            }

            return string.Join("\n", lines.Distinct());
        }

        private static string ExtractMessageText(A2ASdk.Message? message)
        {
            if (message == null)
            {
                return string.Empty;
            }

            var text = message.ToChatMessage().Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            if (message.Parts is not { Count: > 0 })
            {
                return string.Empty;
            }

            return string.Join("\n", message.Parts.Select(ExtractPartText).Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private static string ExtractPartText(A2ASdk.Part? part)
        {
            if (part == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(part.Text))
            {
                return part.Text;
            }

            if (part.Url != null)
            {
                return part.Url.ToString() ?? string.Empty;
            }

            if (part.Data is JsonElement data &&
                data.ValueKind != JsonValueKind.Undefined &&
                data.ValueKind != JsonValueKind.Null)
            {
                return data.ToString();
            }

            if (part.Raw is { Length: > 0 })
            {
                return IsLikelyTextMediaType(part.MediaType)
                    ? Encoding.UTF8.GetString(part.Raw)
                    : $"[binary content, {part.Raw.Length} bytes]";
            }

            return string.Empty;
        }

        private static bool IsLikelyTextMediaType(string? mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
            {
                return true;
            }

            mediaType = mediaType.Trim();
            return mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                mediaType.Contains("json", StringComparison.OrdinalIgnoreCase) ||
                mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase);
        }

        private static string SanitizeToolName(string source, string fallback)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return fallback;
            }

            var sb = new StringBuilder(source.Length);
            foreach (var c in source)
            {
                sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_');
            }

            var result = sb.ToString().Trim('_');
            return string.IsNullOrWhiteSpace(result) ? fallback : result;
        }
    }
}
