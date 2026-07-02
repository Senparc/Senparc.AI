using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.Mcp
{
    /// <summary>
    /// MCP toolset builder.
    /// </summary>
    public static class McpToolsetBuilder
    {
        public const string DefaultPublicBaseUrlEnvName = "MCP_PUBLIC_BASE_URL";

        public const string DefaultSystemPrompt = "You are an assistant that proactively calls MCP tools. For any request that can be completed with tools, prefer calling tools and return the result.";

        /// <summary>
        /// Prepare the MCP tool collection from configuration (for ChatOptions.Tools).
        /// </summary>
        public static async Task<McpToolsetResult> PrepareAsync(McpServerOption option, string? resolvedSseUrl = null)
        {
            ArgumentNullException.ThrowIfNull(option);

            resolvedSseUrl ??= ResolveSseUrl(option);
            if (string.IsNullOrWhiteSpace(resolvedSseUrl))
            {
                throw new InvalidOperationException("No available SSE URL was resolved.");
            }

            if (!Uri.TryCreate(resolvedSseUrl, UriKind.Absolute, out var mcpUri))
            {
                throw new InvalidOperationException($"Invalid SSE URL: {resolvedSseUrl}");
            }

            var bindingMode = option.GetBindingMode();
            var mcpClient = await CreateMcpClientAsync(mcpUri, option.AuthorizationBearerToken);

            try
            {
                var discoveryResult = await TryListMcpToolsAsync(mcpClient);
                var discoveredMcpTools = discoveryResult.Tools;

                var chatTools = BuildChatTools(bindingMode, option, mcpUri, discoveredMcpTools);

                var keepRuntimeClient = bindingMode == McpToolBindingMode.LocalFunctionProxy;
                var result = new McpToolsetResult
                {
                    ServerOption = option,
                    BindingMode = bindingMode,
                    ResolvedSseUrl = resolvedSseUrl,
                    McpUri = mcpUri,
                    DiscoveredMcpTools = discoveredMcpTools,
                    DiscoveredToolNames = discoveredMcpTools.Select(z => z.Name).ToList(),
                    ChatTools = chatTools,
                    ToolDiscoveryError = discoveryResult.Error,
                    RuntimeMcpClient = keepRuntimeClient ? mcpClient : null
                };

                if (!keepRuntimeClient)
                {
                    await mcpClient.DisposeAsync();
                }

                return result;
            }
            catch
            {
                await mcpClient.DisposeAsync();
                throw;
            }
        }

        /// <summary>
        /// Generate common ChatClientAgentOptions for MCP scenarios.
        /// </summary>
        public static ChatClientAgentOptions CreateChatClientAgentOptions(
            IReadOnlyList<AITool> chatTools,
            string? systemPrompt = null,
            string agentName = "McpAgent",
            string agentDescription = "An assistant that can call MCP tools.",
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
        /// Generate ChatClientAgentOptions directly from an MCP build result.
        /// </summary>
        public static ChatClientAgentOptions CreateChatClientAgentOptions(
            this McpToolsetResult result,
            string? systemPrompt = null,
            string agentName = "McpAgent",
            string agentDescription = "An assistant that can call MCP tools.",
            float temperature = 0.2f,
            float topP = 0.2f,
            int maxOutputTokens = 2000)
        {
            ArgumentNullException.ThrowIfNull(result);
            return CreateChatClientAgentOptions(result.ChatTools, systemPrompt, agentName, agentDescription, temperature, topP, maxOutputTokens);
        }

        /// <summary>
        /// Resolve the available SSE address:
        /// SseUrl (public) > LocalSseUrl + PublicBaseUrl/environment variable mapping > LocalSseUrl (original value).
        /// </summary>
        public static string? ResolveSseUrl(McpServerOption option, string publicBaseUrlEnvName = DefaultPublicBaseUrlEnvName)
        {
            ArgumentNullException.ThrowIfNull(option);

            if (!string.IsNullOrWhiteSpace(option.SseUrl))
            {
                return option.SseUrl;
            }

            if (string.IsNullOrWhiteSpace(option.LocalSseUrl))
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
                return option.LocalSseUrl;
            }

            if (!TryMergePublicBaseUrl(publicBaseUrl, option.LocalSseUrl, out var mergedUrl, out _))
            {
                return option.LocalSseUrl;
            }

            return mergedUrl;
        }

        /// <summary>
        /// Replace the local address domain/port with the public Base URL while preserving the original LocalSseUrl path and query parameters.
        /// </summary>
        public static bool TryMergePublicBaseUrl(string publicBaseUrl, string localSseUrl, out string mergedUrl, out string error)
        {
            mergedUrl = localSseUrl;
            error = string.Empty;

            if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var publicUri))
            {
                error = $"PublicBaseUrl is not a valid URL: {publicBaseUrl}";
                return false;
            }

            if (!Uri.TryCreate(localSseUrl, UriKind.Absolute, out var localUri))
            {
                error = $"LocalSseUrl is not a valid URL: {localSseUrl}";
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
        /// Determine whether the URL is a local loopback address.
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

        private static async Task<McpClient> CreateMcpClientAsync(Uri mcpUri, string? bearerToken)
        {
            var transportOptions = new HttpClientTransportOptions
            {
                Endpoint = mcpUri,
                TransportMode = HttpTransportMode.Sse,
                ConnectionTimeout = TimeSpan.FromSeconds(15)
            };

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                transportOptions.AdditionalHeaders["Authorization"] = $"Bearer {bearerToken}";
            }

            var transport = new HttpClientTransport(transportOptions);
            return await McpClient.CreateAsync(transport);
        }

        private static async Task<(IReadOnlyList<McpClientTool> Tools, string? Error)> TryListMcpToolsAsync(McpClient client)
        {
            try
            {
                return ((await client.ListToolsAsync()).ToList(), null);
            }
            catch (Exception ex)
            {
                return ([], ex.Message);
            }
        }

        private static IReadOnlyList<AITool> BuildChatTools(
            McpToolBindingMode bindingMode,
            McpServerOption option,
            Uri mcpUri,
            IReadOnlyList<McpClientTool> discoveredMcpTools)
        {
            if (bindingMode == McpToolBindingMode.HostedServerTool)
            {
                var hosted = BuildHostedMcpTool(option, mcpUri);
                return [hosted];
            }

            return discoveredMcpTools
                .Cast<AITool>()
                .ToList();
        }

        private static HostedMcpServerTool BuildHostedMcpTool(McpServerOption option, Uri mcpUri)
        {
            var tool = new HostedMcpServerTool(option.ServerName, mcpUri);

            if (!string.IsNullOrWhiteSpace(option.ServerDescription))
            {
                tool.ServerDescription = option.ServerDescription;
            }

            if (option.AllowedTools.Count > 0)
            {
                tool.AllowedTools = [.. option.AllowedTools];
            }

            if (!string.IsNullOrWhiteSpace(option.AuthorizationBearerToken) && tool.Headers is { } headers)
            {
                headers["Authorization"] = $"Bearer {option.AuthorizationBearerToken}";
            }

            return tool;
        }
    }
}
