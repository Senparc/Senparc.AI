using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.A2A;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// A2A example: wraps the capabilities of a remote A2A Agent as local AIFunctions (LocalFunctionProxy).
/// </summary>
public class A2ASample
{
    private readonly IAiHandler _aiHandler;

    private const string UserId = "Jeffrey";

    public A2ASample(IAiHandler aiHandler)
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

        Console.WriteLine("A2A Sample: maps remote A2A Agent capabilities to local function tools through LocalFunctionProxy.");
        Console.WriteLine("Tip: To make a local A2A address externally accessible, configure PublicBaseUrl or the A2A_PUBLIC_BASE_URL environment variable.");
        Console.WriteLine();

        var agentOptions = GetAgentOptionsFromSetting();
        if (agentOptions.Count == 0)
        {
            SampleHelper.PrintNote("[Note] No SenparcAiSetting.A2AAgents configuration was found; skipping.");
            PrintConfigTemplate();
            return;
        }

        var selected = ChooseAgent(agentOptions);
        if (selected == null)
        {
            return;
        }

        var resolvedBaseUrl = A2AToolsetBuilder.ResolveBaseUrl(selected);
        if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
        {
            SampleHelper.PrintNote("[Note] No usable BaseUrl could be resolved.");
            PrintAgentFixHint(selected);
            return;
        }
        var effectiveBaseUrl = resolvedBaseUrl;

        if (selected.RequirePublicUrl && A2AToolsetBuilder.IsLocalAddress(effectiveBaseUrl))
        {
            SampleHelper.PrintNote("[Note] A local address was detected. Map it to a public address if external access is required.");
            PrintExposeUrlHint(effectiveBaseUrl);

            Console.WriteLine("Optional: enter a public Base URL, such as https://xxxx.trycloudflare.com, or press Enter to continue using the current address:");
            var runtimePublicBaseUrl = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(runtimePublicBaseUrl))
            {
                if (A2AToolsetBuilder.TryMergePublicBaseUrl(runtimePublicBaseUrl, effectiveBaseUrl, out var mergedUrl, out var error))
                {
                    effectiveBaseUrl = mergedUrl;
                    Console.WriteLine($"[Debug] Runtime public-address mapping succeeded: {effectiveBaseUrl}");
                }
                else
                {
                    Console.WriteLine($"[Debug] Invalid runtime public address: {error}");
                }
            }
        }

        await using var toolset = await A2AToolsetBuilder.PrepareAsync(selected, effectiveBaseUrl);
        PrintToolDiscoveryDebug(selected, toolset);

        var chatOptions = toolset.CreateChatClientAgentOptions(selected.SystemPrompt);
        Console.WriteLine($"[Debug] A2A ToolBindingMode: {toolset.BindingMode}");
        Console.WriteLine($"[Debug] Number of local A2A AIFunctions: {toolset.ChatTools.Count}");
        foreach (var item in toolset.ChatTools)
        {
            Console.WriteLine($"[Debug] Local A2A AIFunction: {item.Name}");
        }

        Console.WriteLine($"[Debug] AgentCard.Name: {toolset.AgentCard.Name}");
        Console.WriteLine($"[Debug] AgentCard.Version: {toolset.AgentCard.Version}");
        Console.WriteLine($"[Debug] Base URL: {toolset.ResolvedBaseUrl}");
        Console.WriteLine("[Debug] Creating AgentSession...");

        var iWantToRun = await agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigChatModel(UserId, chatOptions)
            .BuildKernelWithAgentSessionAsync();

        var session = iWantToRun.Kernel.AgentSession
            ?? throw new InvalidOperationException("Failed to create AgentSession.");

        Console.WriteLine("Configuration complete. Enter exit to quit.");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("Human:");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
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
                Console.WriteLine("An error occurred: " + ex.Message);
                SampleHelper.PrintNote("For A2A-related failures, check BaseUrl reachability, the authorization header, and the AgentCard address.");
            }

            Console.WriteLine();
        }

        Console.WriteLine("A2A sample finished.");
    }

    private static A2AAgentOption? ChooseAgent(IReadOnlyList<A2AAgentOption> agents)
    {
        Console.WriteLine("Select an A2A Agent:");
        var labels = agents.Select(s =>
        {
            var endpoint = s.BaseUrl.IsNullOrEmpty() ? s.LocalBaseUrl : s.BaseUrl;
            return $"{s.Name} ({endpoint})";
        }).ToArray();

        var index = SampleHelper.ChooseItems(labels);
        return index >= 0 && index < agents.Count ? agents[index] : null;
    }

    private static void PrintToolDiscoveryDebug(A2AAgentOption option, A2AToolsetResult toolset)
    {
        Console.WriteLine($"[Debug] Configured AllowedSkills (allowlist): {option.AllowedSkills.Count}");
        if (option.AllowedSkills.Count > 0)
        {
            foreach (var skillName in option.AllowedSkills)
            {
                Console.WriteLine($"[Debug] Allowlisted skill: {skillName}");
            }
        }
        else
        {
            Console.WriteLine("[Debug] AllowedSkills is empty: all AgentCard skills will be loaded while retaining the general send_message tool.");
        }

        if (!toolset.CardDiscoveryError.IsNullOrEmpty())
        {
            Console.WriteLine($"[Debug] AgentCard retrieval note: {toolset.CardDiscoveryError}");
        }

        Console.WriteLine($"[Debug] Actual number of AgentCard skills: {toolset.DiscoveredSkillNames.Count}");
        foreach (var skillName in toolset.DiscoveredSkillNames)
        {
            Console.WriteLine($"[Debug] AgentCard Skill: {skillName}");
        }
    }

    private static void PrintExposeUrlHint(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return;
        }

        var origin = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
        SampleHelper.PrintNote("A reverse tunnel can expose the local address:");
        Console.WriteLine($"- cloudflared: cloudflared tunnel --url {origin}");
        Console.WriteLine($"- ngrok: ngrok http {uri.Port}");
        Console.WriteLine($"When complete, write the public domain to PublicBaseUrl or set the {A2AToolsetBuilder.DefaultPublicBaseUrlEnvName} environment variable.");
        Console.WriteLine();
    }

    private static void PrintConfigTemplate()
    {
        Console.WriteLine("Example configuration (appsettings.json):");
        Console.WriteLine("\"SenparcAiSetting\": { \"A2AAgents\": [ { \"Name\": \"Remote-A2A\", \"BaseUrl\": \"https://your-agent.example.com/a2a\", \"ToolBindingMode\": \"LocalFunctionProxy\" } ] }");
        Console.WriteLine();
    }

    private static void PrintAgentFixHint(A2AAgentOption option)
    {
        Console.WriteLine($"Current configuration: {option.Name}");
        Console.WriteLine("Configure at least one of BaseUrl (public) or LocalBaseUrl (local).");
        Console.WriteLine("When LocalBaseUrl is configured, use PublicBaseUrl or A2A_PUBLIC_BASE_URL for automatic conversion.");
        Console.WriteLine();
    }

    private static IReadOnlyList<A2AAgentOption> GetAgentOptionsFromSetting()
    {
        var current = SampleSetting.CurrentSetting;
        if (current.A2AAgents is { Count: > 0 })
        {
            return current.A2AAgents;
        }

        if (Senparc.AI.Config.SenparcAiSetting?.A2AAgents is { Count: > 0 } rootList)
        {
            return rootList;
        }

        return [];
    }
}
