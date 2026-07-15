using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.A2A;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// A2A 示例：将远端 A2A Agent 的能力包装为本地 AIFunction（LocalFunctionProxy）。
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
            throw new InvalidOperationException("当前示例需要 AgentAiHandler。");
        }

        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("A2A Sample：通过 LocalFunctionProxy 将远端 A2A Agent 能力映射为本地函数工具。");
        Console.WriteLine("提示：如需让外网访问本地 A2A 地址，可配置 PublicBaseUrl 或环境变量 A2A_PUBLIC_BASE_URL。");
        Console.WriteLine();

        var agentOptions = GetAgentOptionsFromSetting();
        if (agentOptions.Count == 0)
        {
            SampleHelper.PrintNote("[提示] 未发现 SenparcAiSetting.A2AAgents 配置，已跳过。");
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
            SampleHelper.PrintNote("[提示] 未解析到可用的 BaseUrl。");
            PrintAgentFixHint(selected);
            return;
        }
        var effectiveBaseUrl = resolvedBaseUrl;

        if (selected.RequirePublicUrl && A2AToolsetBuilder.IsLocalAddress(effectiveBaseUrl))
        {
            SampleHelper.PrintNote("[提示] 检测到本地地址，若需外网访问请映射为公网地址。");
            PrintExposeUrlHint(effectiveBaseUrl);

            Console.WriteLine("可选：请输入公网 Base URL（如 https://xxxx.trycloudflare.com），回车表示继续使用当前地址：");
            var runtimePublicBaseUrl = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(runtimePublicBaseUrl))
            {
                if (A2AToolsetBuilder.TryMergePublicBaseUrl(runtimePublicBaseUrl, effectiveBaseUrl, out var mergedUrl, out var error))
                {
                    effectiveBaseUrl = mergedUrl;
                    Console.WriteLine($"[调试] 运行时公网地址映射成功：{effectiveBaseUrl}");
                }
                else
                {
                    Console.WriteLine($"[调试] 运行时公网地址无效：{error}");
                }
            }
        }

        await using var toolset = await A2AToolsetBuilder.PrepareAsync(selected, effectiveBaseUrl);
        PrintToolDiscoveryDebug(selected, toolset);

        var chatOptions = toolset.CreateChatClientAgentOptions(selected.SystemPrompt);
        Console.WriteLine($"[调试] A2A ToolBindingMode: {toolset.BindingMode}");
        Console.WriteLine($"[调试] A2A Local AIFunction 数量: {toolset.ChatTools.Count}");
        foreach (var item in toolset.ChatTools)
        {
            Console.WriteLine($"[调试] A2A Local AIFunction: {item.Name}");
        }

        Console.WriteLine($"[调试] AgentCard.Name: {toolset.AgentCard.Name}");
        Console.WriteLine($"[调试] AgentCard.Version: {toolset.AgentCard.Version}");
        Console.WriteLine($"[调试] Base URL: {toolset.ResolvedBaseUrl}");
        Console.WriteLine("[调试] 正在创建 AgentSession...");

        var iWantToRun = await agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigChatModel(UserId, chatOptions)
            .BuildKernelWithAgentSessionAsync();

        var session = iWantToRun.Kernel.AgentSession
            ?? throw new InvalidOperationException("AgentSession 创建失败。");

        Console.WriteLine("配置完成。输入 exit 退出。");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("人类：");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
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
                SampleHelper.PrintNote("如与 A2A 调用相关，请检查 BaseUrl 可达性、鉴权头与 AgentCard 地址。");
            }

            Console.WriteLine();
        }

        Console.WriteLine("A2A 示例结束。");
    }

    private static A2AAgentOption? ChooseAgent(IReadOnlyList<A2AAgentOption> agents)
    {
        Console.WriteLine("请选择 A2A Agent：");
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
        Console.WriteLine($"[调试] 配置项 AllowedSkills（白名单）: {option.AllowedSkills.Count}");
        if (option.AllowedSkills.Count > 0)
        {
            foreach (var skillName in option.AllowedSkills)
            {
                Console.WriteLine($"[调试] 配置白名单技能: {skillName}");
            }
        }
        else
        {
            Console.WriteLine("[调试] 配置 AllowedSkills 为空：将读取 AgentCard 的全部 skills（同时保留通用 send_message 工具）。");
        }

        if (!toolset.CardDiscoveryError.IsNullOrEmpty())
        {
            Console.WriteLine($"[调试] AgentCard 获取提示：{toolset.CardDiscoveryError}");
        }

        Console.WriteLine($"[调试] AgentCard 实际 skills 数: {toolset.DiscoveredSkillNames.Count}");
        foreach (var skillName in toolset.DiscoveredSkillNames)
        {
            Console.WriteLine($"[调试] AgentCard Skill: {skillName}");
        }
    }

    private static void PrintExposeUrlHint(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return;
        }

        var origin = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
        SampleHelper.PrintNote("可使用反向隧道暴露本地地址：");
        Console.WriteLine($"- cloudflared: cloudflared tunnel --url {origin}");
        Console.WriteLine($"- ngrok: ngrok http {uri.Port}");
        Console.WriteLine($"完成后将公网域名写入 PublicBaseUrl，或设置环境变量 {A2AToolsetBuilder.DefaultPublicBaseUrlEnvName}。");
        Console.WriteLine();
    }

    private static void PrintConfigTemplate()
    {
        Console.WriteLine("示例配置（appsettings.json）：");
        Console.WriteLine("\"SenparcAiSetting\": { \"A2AAgents\": [ { \"Name\": \"Remote-A2A\", \"BaseUrl\": \"https://your-agent.example.com/a2a\", \"ToolBindingMode\": \"LocalFunctionProxy\" } ] }");
        Console.WriteLine();
    }

    private static void PrintAgentFixHint(A2AAgentOption option)
    {
        Console.WriteLine($"当前配置：{option.Name}");
        Console.WriteLine("请至少配置 BaseUrl（公网）或 LocalBaseUrl（本地）之一。");
        Console.WriteLine("如配置 LocalBaseUrl，可配合 PublicBaseUrl / A2A_PUBLIC_BASE_URL 自动转换。");
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
