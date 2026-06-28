using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Mcp;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// MCP 示例：支持 LocalFunctionProxy / HostedServerTool 两种工具绑定模式。
/// </summary>
public class McpSample
{
    private readonly IAiHandler _aiHandler;

    private const string UserId = "Jeffrey";

    public McpSample(IAiHandler aiHandler)
    {
        _aiHandler = aiHandler;
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

        var serverOptions = GetServerOptionsFromSetting();
        if (serverOptions.Count == 0)
        {
            SampleHelper.PrintNote("[提示] 未发现 SenparcAiSetting.McpServers 配置，已跳过。");
            PrintConfigTemplate();
            return;
        }

        var selected = ChooseServer(serverOptions);
        if (selected == null)
        {
            return;
        }

        var bindingMode = selected.GetBindingMode();
        var resolvedSseUrl = McpToolsetBuilder.ResolveSseUrl(selected);
        if (resolvedSseUrl.IsNullOrEmpty())
        {
            SampleHelper.PrintNote("[提示] 未解析到可用的 SSE URL。");
            PrintServerFixHint(selected);
            return;
        }

        if (bindingMode == McpToolBindingMode.HostedServerTool &&
            McpToolsetBuilder.IsLocalAddress(resolvedSseUrl) &&
            selected.RequirePublicUrl)
        {
            SampleHelper.PrintNote("[提示] 检测到本地地址。Hosted MCP 由模型服务端访问，通常无法直连 localhost。");
            PrintExposeUrlHint(resolvedSseUrl);

            Console.WriteLine("可选：请输入公网 Base URL（如 https://xxxx.trycloudflare.com），回车表示继续使用当前地址：");
            var runtimePublicBaseUrl = Console.ReadLine();
            if (!runtimePublicBaseUrl.IsNullOrEmpty())
            {
                if (McpToolsetBuilder.TryMergePublicBaseUrl(runtimePublicBaseUrl, resolvedSseUrl, out var mergedUrl, out var error))
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

        var toolset = await McpToolsetBuilder.PrepareAsync(selected, resolvedSseUrl);
        PrintToolDiscoveryDebug(selected, toolset);

        var chatOptions = toolset.CreateChatClientAgentOptions(selected.SystemPrompt);

        Console.WriteLine($"[调试] MCP ToolBindingMode: {toolset.BindingMode}");
        if (toolset.BindingMode == McpToolBindingMode.HostedServerTool)
        {
            var mcpTool = toolset.ChatTools.OfType<HostedMcpServerTool>().FirstOrDefault();
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
            Console.WriteLine($"[调试] Local MCP AIFunction 数量: {toolset.ChatTools.Count}");
            foreach (var item in toolset.ChatTools)
            {
                Console.WriteLine($"[调试] Local MCP AIFunction: {item.Name}");
            }
        }

        Console.WriteLine($"[调试] MCP Server: {selected.ServerName}");
        Console.WriteLine($"[调试] SSE URL: {toolset.ResolvedSseUrl}");
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

    private static void PrintToolDiscoveryDebug(McpServerOption option, McpToolsetResult toolset)
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
            Console.WriteLine(toolset.BindingMode == McpToolBindingMode.HostedServerTool
                ? "[调试] 配置 AllowedTools 为空：Hosted MCP 将不限制工具名（由模型服务端连接 MCP Server 时使用全部工具）。"
                : "[调试] 配置 AllowedTools 为空：LocalFunctionProxy 将加载 MCP Server 的全部工具。");
        }

        if (!toolset.ToolDiscoveryError.IsNullOrEmpty())
        {
            Console.WriteLine($"[调试] 无法从 MCP Server 探测工具列表：{toolset.ToolDiscoveryError}");
        }

        Console.WriteLine($"[调试] MCP Server 实际工具数: {toolset.DiscoveredToolNames.Count}");
        foreach (var toolName in toolset.DiscoveredToolNames)
        {
            Console.WriteLine($"[调试] MCP Server 工具: {toolName}");
        }
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
        Console.WriteLine($"完成后将公网域名写入 PublicBaseUrl，或设置环境变量 {McpToolsetBuilder.DefaultPublicBaseUrlEnvName}。");
        Console.WriteLine();
    }

    private static void PrintConfigTemplate()
    {
        Console.WriteLine("示例配置（appsettings.json）：");
        Console.WriteLine("\"SenparcAiSetting\": { \"McpServers\": [ { \"Name\": \"Demo\", \"ServerName\": \"demo\", \"LocalSseUrl\": \"http://127.0.0.1:3001/sse\", \"ToolBindingMode\": \"LocalFunctionProxy\" } ] }");
        Console.WriteLine();
    }

    private static void PrintServerFixHint(McpServerOption option)
    {
        Console.WriteLine($"当前配置：{option.Name}");
        Console.WriteLine("请至少配置 SseUrl（公网）或 LocalSseUrl（本地）之一。");
        Console.WriteLine("如配置 LocalSseUrl，可配合 PublicBaseUrl / MCP_PUBLIC_BASE_URL 自动转换。");
        Console.WriteLine();
    }

    private static IReadOnlyList<McpServerOption> GetServerOptionsFromSetting()
    {
        var current = SampleSetting.CurrentSetting;
        if (current.McpServers is { Count: > 0 })
        {
            return current.McpServers;
        }

        if (Senparc.AI.Config.SenparcAiSetting?.McpServers is { Count: > 0 } rootList)
        {
            return rootList;
        }

        return [];
    }
}
