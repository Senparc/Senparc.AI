using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using System.Net;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// MCP 示例（Hosted MCP Server Tool）。
/// </summary>
public class McpSample
{
    private readonly IAiHandler _aiHandler;
    private readonly IConfiguration _configuration;

    private const string UserId = "Jeffrey";
    private const string PublicBaseUrlEnvName = "MCP_PUBLIC_BASE_URL";

    public McpSample(IAiHandler aiHandler, IConfiguration configuration)
    {
        _aiHandler = aiHandler;
        _configuration = configuration;
        if (aiHandler is AgentAiHandler h)
        {
            h.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);
        }
    }

    public async Task RunAsync()
    {
        if (_aiHandler is not AgentAiHandler agentHandler)
        {
            throw new InvalidOperationException("当前示例需要 AgentAiHandler。");
        }

        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("MCP Sample：通过 HostedMcpServerTool 使用 MCP Server。");
        Console.WriteLine("提示：当 SSE 地址是 localhost 时，通常需要先映射为公网地址。");
        Console.WriteLine();

        var serverOptions = LoadServerOptions();
        if (serverOptions.Count == 0)
        {
            SampleHelper.PrintNote("[提示] 未发现 McpSample.Servers 配置，已跳过。");
            PrintConfigTemplate();
            return;
        }

        var selected = ChooseServer(serverOptions);
        if (selected == null)
        {
            return;
        }

        var resolvedSseUrl = ResolveSseUrl(selected);
        if (resolvedSseUrl.IsNullOrEmpty())
        {
            SampleHelper.PrintNote("[提示] 未解析到可用的 SSE URL。");
            PrintServerFixHint(selected);
            return;
        }

        if (IsLocalAddress(resolvedSseUrl) && selected.RequirePublicUrl)
        {
            SampleHelper.PrintNote("[提示] 检测到本地地址。Hosted MCP 由模型服务端访问，通常无法直连 localhost。");
            PrintExposeUrlHint(resolvedSseUrl);

            Console.WriteLine("可选：请输入公网 Base URL（如 https://xxxx.trycloudflare.com），回车表示继续使用当前地址：");
            var runtimePublicBaseUrl = Console.ReadLine();
            if (!runtimePublicBaseUrl.IsNullOrEmpty())
            {
                if (TryMergePublicBaseUrl(runtimePublicBaseUrl, resolvedSseUrl, out var mergedUrl, out var error))
                {
                    resolvedSseUrl = mergedUrl;
                    Console.WriteLine($"[调试] 运行时公网地址映射成功：{resolvedSseUrl}");
                }
                else
                {
                    Console.WriteLine($"[调试] 运行时公网地址无效：{error}");
                }
            }
        }

        if (!Uri.TryCreate(resolvedSseUrl, UriKind.Absolute, out var mcpUri))
        {
            Console.WriteLine($"SSE URL 无效：{resolvedSseUrl}");
            return;
        }

        var mcpTool = BuildHostedMcpTool(selected, mcpUri);
        var chatOptions = new ChatClientAgentOptions
        {
            Name = "McpAgent",
            Description = "An assistant that can call hosted MCP tools.",
            ChatOptions = new ChatOptions
            {
                Instructions = selected.SystemPrompt.IsNullOrEmpty()
                    ? "你是一个会主动调用 MCP 工具的助手。凡是可以通过工具完成的问题，优先调用工具并给出结果。"
                    : selected.SystemPrompt,
                Temperature = 0.2f,
                TopP = 0.2f,
                MaxOutputTokens = 2000,
                Tools = [mcpTool],
                AllowMultipleToolCalls = true
            }
        };

        //output all tools of the mcpTool
        Console.WriteLine($"[调试] MCP Tools: {mcpTool.AllowedTools?.Count() ?? -1}");
        foreach (var tool in mcpTool.AllowedTools)
        {
            Console.WriteLine($"[调试] MCP Tool: {tool}");
        }

        Console.WriteLine($"[调试] MCP Server: {selected.ServerName}");
        Console.WriteLine($"[调试] SSE URL: {resolvedSseUrl}");
        Console.WriteLine("[调试] 正在创建 AgentSession...");

        var iWantToRun = await agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigChatModel(UserId, chatOptions)
            .BuildKernelWithAgentSessionAsync();

        var session = iWantToRun.Kernel.AgentSession;

        Console.WriteLine("配置完成。输入 exit 退出。");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("人类：");
            var input = Console.ReadLine();
            if (input.IsNullOrEmpty())
            {
                continue;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine("机器：");
            try
            {
                var result = await iWantToRun.RunChatAsync(input, session);
                Console.WriteLine(result.Result.Text);
                Console.WriteLine($"[调试] Tokens — total: {result.Result.Usage?.TotalTokenCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生错误：" + ex.Message);
                SampleHelper.PrintNote("如与 MCP 调用相关，请检查 SSE URL 可达性、鉴权头和模型平台支持情况。");
            }

            Console.WriteLine();
        }

        Console.WriteLine("MCP 示例结束。");
    }

    private HostedMcpServerTool BuildHostedMcpTool(McpServerOption option, Uri mcpUri)
    {
        var tool = new HostedMcpServerTool(option.ServerName, mcpUri);

        if (!option.ServerDescription.IsNullOrEmpty())
        {
            tool.ServerDescription = option.ServerDescription;
        }
Console.WriteLine($"[调试] AllowedTools: {option.AllowedTools?.Count() ?? -1}");
Console.WriteLine($"[调试] {option.ToJson(true)}");

        if (option.AllowedTools.Count > 0 && tool.AllowedTools is { } allowedTools)
        {
            foreach (var toolName in option.AllowedTools)
            {
                allowedTools.Add(toolName);
            }
        }

        if (!option.AuthorizationBearerToken.IsNullOrEmpty() && tool.Headers is { } headers)
        {
            headers["Authorization"] = $"Bearer {option.AuthorizationBearerToken}";
        }

        return tool;
    }

    private IReadOnlyList<McpServerOption> LoadServerOptions()
    {
        var list = new List<McpServerOption>();
        var serverSections = _configuration.GetSection("McpSample").GetSection("Servers").GetChildren();

        foreach (var section in serverSections)
        {
            var name = section["Name"] ?? section["ServerName"] ?? "McpServer";
            var serverName = section["ServerName"] ?? name;
            var sseUrl = section["SseUrl"];
            var localSseUrl = section["LocalSseUrl"];

            var allowedTools = section.GetSection("AllowedTools")
                .GetChildren()
                .Select(s => s.Value)
                .Where(v => !v.IsNullOrEmpty())
                .ToList();

            if (serverName.IsNullOrEmpty() || (sseUrl.IsNullOrEmpty() && localSseUrl.IsNullOrEmpty()))
            {
                continue;
            }

            list.Add(new McpServerOption
            {
                Name = name,
                ServerName = serverName,
                SseUrl = sseUrl,
                LocalSseUrl = localSseUrl,
                PublicBaseUrl = section["PublicBaseUrl"],
                ServerDescription = section["ServerDescription"],
                SystemPrompt = section["SystemPrompt"],
                AuthorizationBearerToken = section["AuthorizationBearerToken"],
                RequirePublicUrl = false,//ParseBool(section["RequirePublicUrl"], true),
                AllowedTools = allowedTools
            });
        }

        return list;
    }

    private static McpServerOption? ChooseServer(IReadOnlyList<McpServerOption> servers)
    {
        Console.WriteLine("请选择 MCP Server：");
        var labels = servers.Select(s =>
        {
            var endpoint = s.SseUrl.IsNullOrEmpty() ? s.LocalSseUrl : s.SseUrl;
            return $"{s.Name} ({endpoint})";
        }).ToArray();

        var index = SampleHelper.ChooseItems(labels);
        return index >= 0 && index < servers.Count ? servers[index] : null;
    }

    private static bool ParseBool(string? value, bool defaultValue)
    {
        return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    private static string? ResolveSseUrl(McpServerOption option)
    {
        if (!option.SseUrl.IsNullOrEmpty())
        {
            return option.SseUrl;
        }

        if (option.LocalSseUrl.IsNullOrEmpty())
        {
            return null;
        }

        var publicBaseUrl = option.PublicBaseUrl;
        if (publicBaseUrl.IsNullOrEmpty())
        {
            publicBaseUrl = Environment.GetEnvironmentVariable(PublicBaseUrlEnvName);
        }

        if (publicBaseUrl.IsNullOrEmpty())
        {
            return option.LocalSseUrl;
        }

        if (!TryMergePublicBaseUrl(publicBaseUrl, option.LocalSseUrl, out var mergedUrl, out _))
        {
            return option.LocalSseUrl;
        }

        return mergedUrl;
    }

    private static bool TryMergePublicBaseUrl(string publicBaseUrl, string localSseUrl, out string mergedUrl, out string error)
    {
        mergedUrl = localSseUrl;
        error = string.Empty;

        if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var publicUri))
        {
            error = $"PublicBaseUrl 不是合法 URL：{publicBaseUrl}";
            return false;
        }

        if (!Uri.TryCreate(localSseUrl, UriKind.Absolute, out var localUri))
        {
            error = $"LocalSseUrl 不是合法 URL：{localSseUrl}";
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

    private static bool IsLocalAddress(string url)
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

    private static void PrintExposeUrlHint(string sseUrl)
    {
        if (!Uri.TryCreate(sseUrl, UriKind.Absolute, out var uri))
        {
            return;
        }

        var origin = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
        SampleHelper.PrintNote("可使用反向隧道暴露本地地址：");
        Console.WriteLine($"- cloudflared: cloudflared tunnel --url {origin}");
        Console.WriteLine($"- ngrok: ngrok http {uri.Port}");
        Console.WriteLine($"完成后将公网域名写入 PublicBaseUrl，或设置环境变量 {PublicBaseUrlEnvName}。");
        Console.WriteLine();
    }

    private static void PrintConfigTemplate()
    {
        Console.WriteLine("示例配置（appsettings.json）：");
        Console.WriteLine("\"McpSample\": { \"Servers\": [ { \"Name\": \"Demo\", \"ServerName\": \"demo\", \"LocalSseUrl\": \"http://127.0.0.1:3001/sse\", \"PublicBaseUrl\": \"https://xxxx.trycloudflare.com\" } ] }");
        Console.WriteLine();
    }

    private static void PrintServerFixHint(McpServerOption option)
    {
        Console.WriteLine($"当前配置：{option.Name}");
        Console.WriteLine("请至少配置 SseUrl（公网）或 LocalSseUrl（本地）之一。");
        Console.WriteLine("如配置 LocalSseUrl，可配合 PublicBaseUrl / MCP_PUBLIC_BASE_URL 自动转换。");
        Console.WriteLine();
    }

    private sealed class McpServerOption
    {
        public string Name { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public string? SseUrl { get; set; }
        public string? LocalSseUrl { get; set; }
        public string? PublicBaseUrl { get; set; }
        public string? ServerDescription { get; set; }
        public string? SystemPrompt { get; set; }
        public string? AuthorizationBearerToken { get; set; }
        public bool RequirePublicUrl { get; set; } = true;
        public List<string> AllowedTools { get; set; } = new();
    }
}
