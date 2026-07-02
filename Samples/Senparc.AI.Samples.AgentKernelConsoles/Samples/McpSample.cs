using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Mcp;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// MCP sample: supports LocalFunctionProxy and HostedServerTool tool binding modes.
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
            throw new InvalidOperationException("This sample requires AgentAiHandler.");
        }

        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("MCP Sample: supports LocalFunctionProxy and HostedServerTool modes.");
        Console.WriteLine("Tip: in HostedServerTool mode, a localhost SSE address usually needs to be mapped to a public address.");
        Console.WriteLine();

        var serverOptions = GetServerOptionsFromSetting();
        if (serverOptions.Count == 0)
        {
            SampleHelper.PrintNote("[Note] SenparcAiSetting.McpServers configuration was not found. Skipped.");
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
            SampleHelper.PrintNote("[Note] No available SSE URL was resolved.");
            PrintServerFixHint(selected);
            return;
        }

        if (bindingMode == McpToolBindingMode.HostedServerTool &&
            McpToolsetBuilder.IsLocalAddress(resolvedSseUrl) &&
            selected.RequirePublicUrl)
        {
            SampleHelper.PrintNote("[Note] Local address detected. Hosted MCP is accessed by the model server and usually cannot connect directly to localhost.");
            PrintExposeUrlHint(resolvedSseUrl);

            Console.WriteLine("Optional: enter a public Base URL, such as https://xxxx.trycloudflare.com. Press Enter to keep using the current address:");
            var runtimePublicBaseUrl = Console.ReadLine();
            if (!runtimePublicBaseUrl.IsNullOrEmpty())
            {
                if (McpToolsetBuilder.TryMergePublicBaseUrl(runtimePublicBaseUrl, resolvedSseUrl, out var mergedUrl, out var error))
                {
                    resolvedSseUrl = mergedUrl;
                    Console.WriteLine($"[Debug] Runtime public address mapping succeeded: {resolvedSseUrl}");
                }
                else
                {
                    Console.WriteLine($"[Debug] Runtime public address is invalid: {error}");
                }
            }
        }

        await using var toolset = await McpToolsetBuilder.PrepareAsync(selected, resolvedSseUrl);
        PrintToolDiscoveryDebug(selected, toolset);

        var chatOptions = toolset.CreateChatClientAgentOptions(selected.SystemPrompt);

        Console.WriteLine($"[Debug] MCP ToolBindingMode: {toolset.BindingMode}");
        if (toolset.BindingMode == McpToolBindingMode.HostedServerTool)
        {
            var mcpTool = toolset.ChatTools.OfType<HostedMcpServerTool>().FirstOrDefault();
            var hostedAllowedTools = mcpTool?.AllowedTools;
            Console.WriteLine($"[Debug] HostedMcpServerTool.AllowedTools: {(hostedAllowedTools == null ? "null (unrestricted, allows all server-side tools)" : hostedAllowedTools.Count.ToString())}");
            if (hostedAllowedTools is { Count: > 0 })
            {
                foreach (var toolName in hostedAllowedTools)
                {
                    Console.WriteLine($"[Debug] Hosted allowlisted tool: {toolName}");
                }
            }
        }
        else
        {
            Console.WriteLine($"[Debug] Local MCP AIFunction count: {toolset.ChatTools.Count}");
            foreach (var item in toolset.ChatTools)
            {
                Console.WriteLine($"[Debug] Local MCP AIFunction: {item.Name}");
            }
        }

        Console.WriteLine($"[Debug] MCP Server: {selected.ServerName}");
        Console.WriteLine($"[Debug] SSE URL: {toolset.ResolvedSseUrl}");
        Console.WriteLine("[Debug] Creating AgentSession...");

        var iWantToRun = await agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigChatModel(UserId, chatOptions)
            .BuildKernelWithAgentSessionAsync();

        var session = iWantToRun.Kernel.AgentSession;

        Console.WriteLine("Configuration completed. Enter exit to leave.");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("Human:");
            var input = Console.ReadLine();
            if (input.IsNullOrEmpty())
            {
                continue;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine("Assistant:");
            try
            {
                var result = await iWantToRun.RunChatAsync(input, session);
                Console.WriteLine(result.Result.Text);
                Console.WriteLine($"[Debug] Tokens - total: {result.Result.Usage?.TotalTokenCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:" + ex.Message);
                SampleHelper.PrintNote("If this is related to MCP calls, check SSE URL reachability, authentication headers, and model platform support.");
            }

            Console.WriteLine();
        }

        Console.WriteLine("MCP sample ended.");
    }

    private static McpServerOption? ChooseServer(IReadOnlyList<McpServerOption> servers)
    {
        Console.WriteLine("Select an MCP Server:");
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
        Console.WriteLine($"[Debug] Configured AllowedTools (allowlist): {option.AllowedTools.Count}");
        if (option.AllowedTools.Count > 0)
        {
            foreach (var toolName in option.AllowedTools)
            {
                Console.WriteLine($"[Debug] Configured allowlisted tool: {toolName}");
            }
        }
        else
        {
            Console.WriteLine(toolset.BindingMode == McpToolBindingMode.HostedServerTool
                ? "[Debug] Configured AllowedTools is empty: Hosted MCP will not restrict tool names (all tools are used when the model server connects to the MCP Server)."
                : "[Debug] Configured AllowedTools is empty: LocalFunctionProxy will load all tools from the MCP Server.");
        }

        if (!toolset.ToolDiscoveryError.IsNullOrEmpty())
        {
            Console.WriteLine($"[Debug] Unable to discover the tool list from the MCP Server: {toolset.ToolDiscoveryError}");
        }

        Console.WriteLine($"[Debug] Actual MCP Server tool count: {toolset.DiscoveredToolNames.Count}");
        foreach (var toolName in toolset.DiscoveredToolNames)
        {
            Console.WriteLine($"[Debug] MCP Server tool: {toolName}");
        }
    }

    private static void PrintExposeUrlHint(string sseUrl)
    {
        if (!Uri.TryCreate(sseUrl, UriKind.Absolute, out var uri))
        {
            return;
        }

        var origin = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
        SampleHelper.PrintNote("A reverse tunnel can expose the local address:");
        Console.WriteLine($"- cloudflared: cloudflared tunnel --url {origin}");
        Console.WriteLine($"- ngrok: ngrok http {uri.Port}");
        Console.WriteLine($"After completion, write the public domain to PublicBaseUrl, or set the environment variable {McpToolsetBuilder.DefaultPublicBaseUrlEnvName}.");
        Console.WriteLine();
    }

    private static void PrintConfigTemplate()
    {
        Console.WriteLine("Sample configuration (appsettings.json):");
        Console.WriteLine("\"SenparcAiSetting\": { \"McpServers\": [ { \"Name\": \"Demo\", \"ServerName\": \"demo\", \"LocalSseUrl\": \"http://127.0.0.1:3001/sse\", \"ToolBindingMode\": \"LocalFunctionProxy\" } ] }");
        Console.WriteLine();
    }

    private static void PrintServerFixHint(McpServerOption option)
    {
        Console.WriteLine($"Current configuration:{option.Name}");
        Console.WriteLine("Configure at least one of SseUrl (public) or LocalSseUrl (local).");
        Console.WriteLine("If LocalSseUrl is configured, PublicBaseUrl / MCP_PUBLIC_BASE_URL can be used for automatic conversion.");
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
