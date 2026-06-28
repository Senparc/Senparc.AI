using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using System.Net;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// MCP 示例：支持 LocalFunctionProxy / HostedServerTool 两种工具绑定模式。
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

        Console.WriteLine("MCP Sample：支持 LocalFunctionProxy / HostedServerTool 两种模式。");
        Console.WriteLine("提示：HostedServerTool 模式下，当 SSE 地址是 localhost 时通常需要映射公网地址。");
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

        // 绑定模式决定“工具由谁执行”：
        // LocalFunctionProxy：模型调用本地 AIFunction（本进程转发到 MCP）。
        // HostedServerTool：模型服务端直接连接 MCP Server。
        var bindingMode = selected.GetBindingMode();

        // SSE URL 解析优先级：
        // SseUrl(公网) > LocalSseUrl + PublicBaseUrl/环境变量映射 > LocalSseUrl(原值)。
        var resolvedSseUrl = ResolveSseUrl(selected);
        if (resolvedSseUrl.IsNullOrEmpty())
        {
            SampleHelper.PrintNote("[提示] 未解析到可用的 SSE URL。");
            PrintServerFixHint(selected);
            return;
        }

        if (bindingMode == McpToolBindingMode.HostedServerTool &&
            IsLocalAddress(resolvedSseUrl) &&
            selected.RequirePublicUrl)
        {
            // Hosted 模式下，MCP 连接发生在“模型服务端”，localhost 通常不可达。
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

        // 建立 MCP 客户端用于探测工具；Local 模式下也用于后续本地代理调用。
        await using var mcpClient = await CreateMcpClientAsync(mcpUri, selected.AuthorizationBearerToken);
        var discoveredMcpTools = await ListMcpToolsAsync(mcpClient);
        var discoveredToolNames = discoveredMcpTools.Select(z => z.Name).ToList();
        PrintToolDiscoveryDebug(selected, bindingMode, discoveredToolNames);

        // 根据绑定模式构造最终传给 ChatOptions.Tools 的工具集合。
        var chatTools = BuildChatTools(bindingMode, selected, mcpUri, discoveredMcpTools);
        var chatOptions = new ChatClientAgentOptions
        {
            Name = "McpAgent",
            Description = "An assistant that can call MCP tools.",
            ChatOptions = new ChatOptions
            {
                Instructions = selected.SystemPrompt.IsNullOrEmpty()
                    ? "你是一个会主动调用 MCP 工具的助手。凡是可以通过工具完成的问题，优先调用工具并给出结果。"
                    : selected.SystemPrompt,
                Temperature = 0.2f,
                TopP = 0.2f,
                MaxOutputTokens = 2000,
                Tools = chatTools,
                AllowMultipleToolCalls = true
            }
        };

        Console.WriteLine($"[调试] MCP ToolBindingMode: {bindingMode}");
        if (bindingMode == McpToolBindingMode.HostedServerTool)
        {
            var mcpTool = chatTools.OfType<HostedMcpServerTool>().FirstOrDefault();
            var hostedAllowedTools = mcpTool?.AllowedTools;
            Console.WriteLine($"[调试] HostedMcpServerTool.AllowedTools: {(hostedAllowedTools == null ? "null（不限制，允许服务端全部工具）" : hostedAllowedTools.Count.ToString())}");
            if (hostedAllowedTools is { Count: > 0 })
            {
                foreach (var toolName in hostedAllowedTools)
                {
                    Console.WriteLine($"[调试] Hosted 白名单工具: {toolName}");
                }
            }
        }
        else
        {
            Console.WriteLine($"[调试] Local MCP AIFunction 数量: {chatTools.Count}");
            foreach (var item in chatTools)
            {
                Console.WriteLine($"[调试] Local MCP AIFunction: {item.Name}");
            }
        }

        Console.WriteLine($"[调试] MCP Server: {selected.ServerName}");
        Console.WriteLine($"[调试] SSE URL: {resolvedSseUrl}");
        Console.WriteLine("[调试] 正在创建 AgentSession...");

        // 将 MCP 工具（Hosted 或 Local）绑定到当前会话。
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

    private static List<AITool> BuildChatTools(
        McpToolBindingMode bindingMode,
        McpServerOption option,
        Uri mcpUri,
        IReadOnlyList<McpClientTool> discoveredMcpTools)
    {
        if (bindingMode == McpToolBindingMode.HostedServerTool)
        {
            // Hosted 模式：仅暴露一个 HostedMcpServerTool，由模型服务端负责调用远端 MCP。
            var hosted = BuildHostedMcpTool(option, mcpUri);
            return [hosted];
        }

        // 使用本地代理模式：将 MCP 工具作为 AIFunction 提供给模型，调用由当前进程完成。
        return discoveredMcpTools
            .Cast<AITool>()
            .ToList();
    }

    private static HostedMcpServerTool BuildHostedMcpTool(McpServerOption option, Uri mcpUri)
    {
        var tool = new HostedMcpServerTool(option.ServerName, mcpUri);

        if (!option.ServerDescription.IsNullOrEmpty())
        {
            tool.ServerDescription = option.ServerDescription;
        }

        // AllowedTools 来自配置白名单；为空时不设置 HostedMcpServerTool.AllowedTools（保持 null = 允许全部工具）。
        if (option.AllowedTools.Count > 0)
        {
            tool.AllowedTools = [.. option.AllowedTools];
        }

        if (!option.AuthorizationBearerToken.IsNullOrEmpty() && tool.Headers is { } headers)
        {
            headers["Authorization"] = $"Bearer {option.AuthorizationBearerToken}";
        }

        return tool;
    }

    /// <summary>
    /// 输出“配置白名单”和“服务端真实探测结果”的对照，便于快速定位工具未生效问题。
    /// </summary>
    private static void PrintToolDiscoveryDebug(McpServerOption option, McpToolBindingMode bindingMode, IReadOnlyList<string> discoveredTools)
    {
        Console.WriteLine($"[调试] 配置项 AllowedTools（白名单）: {option.AllowedTools.Count}");
        if (option.AllowedTools.Count > 0)
        {
            foreach (var toolName in option.AllowedTools)
            {
                Console.WriteLine($"[调试] 配置白名单工具: {toolName}");
            }
        }
        else
        {
            Console.WriteLine(bindingMode == McpToolBindingMode.HostedServerTool
                ? "[调试] 配置 AllowedTools 为空：Hosted MCP 将不限制工具名（由模型服务端连接 MCP Server 时使用全部工具）。"
                : "[调试] 配置 AllowedTools 为空：LocalFunctionProxy 将加载 MCP Server 的全部工具。");
        }

        Console.WriteLine($"[调试] MCP Server 实际工具数: {discoveredTools.Count}");
        foreach (var toolName in discoveredTools)
        {
            Console.WriteLine($"[调试] MCP Server 工具: {toolName}");
        }
    }

    /// <summary>
    /// 通过 SSE 传输创建 MCP 客户端。
    /// </summary>
    private static async Task<McpClient> CreateMcpClientAsync(Uri mcpUri, string? bearerToken)
    {
        var transportOptions = new HttpClientTransportOptions
        {
            Endpoint = mcpUri,
            TransportMode = HttpTransportMode.Sse,
            ConnectionTimeout = TimeSpan.FromSeconds(15)
        };

        if (!bearerToken.IsNullOrEmpty())
        {
            transportOptions.AdditionalHeaders["Authorization"] = $"Bearer {bearerToken}";
        }

        var transport = new HttpClientTransport(transportOptions);
        return await McpClient.CreateAsync(transport);
    }

    /// <summary>
    /// 尝试探测 MCP Server 的工具列表；失败时返回空集合并打印调试信息。
    /// </summary>
    private static async Task<IReadOnlyList<McpClientTool>> ListMcpToolsAsync(McpClient client)
    {
        try
        {
            return (IReadOnlyList<McpClientTool>)await client.ListToolsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[调试] 无法从 MCP Server 探测工具列表：{ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// 读取 McpSample.Servers 配置并做最小合法性过滤。
    /// </summary>
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
                RequirePublicUrl = ParseBool(section["RequirePublicUrl"], true),
                ToolBindingMode = section["ToolBindingMode"],
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

    /// <summary>
    /// 解析可用 SSE 地址：
    /// 1) 优先 SseUrl（公网）；
    /// 2) 否则尝试 LocalSseUrl + PublicBaseUrl（配置或环境变量）合成；
    /// 3) 失败则回退 LocalSseUrl。
    /// </summary>
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

    /// <summary>
    /// 用公网 Base URL 替换本地地址的域名/端口，保留原 LocalSseUrl 的路径和查询参数。
    /// </summary>
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

    /// <summary>
    /// 提示如何将本地 MCP SSE 地址通过隧道暴露为公网地址。
    /// </summary>
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
        Console.WriteLine("\"McpSample\": { \"Servers\": [ { \"Name\": \"Demo\", \"ServerName\": \"demo\", \"LocalSseUrl\": \"http://127.0.0.1:3001/sse\", \"ToolBindingMode\": \"LocalFunctionProxy\" } ] }");
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
        public string? ToolBindingMode { get; set; }
        public bool RequirePublicUrl { get; set; } = true;
        public List<string> AllowedTools { get; set; } = new();

        public McpToolBindingMode GetBindingMode()
        {
            // 默认值选择 LocalFunctionProxy，保证在配置缺失时仍可进行本地调试。
            return Enum.TryParse<McpToolBindingMode>(ToolBindingMode, ignoreCase: true, out var parsed)
                ? parsed
                : McpToolBindingMode.LocalFunctionProxy;
        }
    }

    /// <summary>
    /// MCP 工具绑定模式。
    /// </summary>
    private enum McpToolBindingMode
    {
        /// <summary>
        /// 本地函数代理：模型调用本地函数，本进程再转发到 MCP。
        /// </summary>
        LocalFunctionProxy = 0,
        /// <summary>
        /// Hosted MCP：模型服务端直接连接并调用 MCP Server。
        /// </summary>
        HostedServerTool = 1,
    }
}
