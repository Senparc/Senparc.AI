/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    文件名：ChatSample.cs
    文件功能描述：演示基于 AgentKernel 的多轮聊天会话配置与交互流程。


    创建标识：Senparc - 20260521

    修改标识：Senparc - 20260813
    修改描述：v1.3.0 增加严格 IWantToRun 执行模式示例

----------------------------------------------------------------*/
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// 多轮对话示例，参考 AgentAiHandlerTests.ConversationTestWithDefaultSession。
/// </summary>
public class ChatSample
{
    private readonly IAiHandler _aiHandler;

    public ChatSample(IAiHandler aiHandler)
    {
        _aiHandler = aiHandler;
        SyncHttpClientLog();
    }

    private void SyncHttpClientLog()
    {
        if (_aiHandler is AgentAiHandler h)
        {
            h.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);
        }
    }

    private static AgentAiHandler GetHandler(IAiHandler handler)
    {
        if (handler is AgentAiHandler agentHandler)
        {
            return agentHandler;
        }

        throw new InvalidOperationException("当前示例需要 AgentAiHandler，请确认已调用 AddSenparcAI。");
    }

    public async Task RunAsync()
    {
        SyncHttpClientLog();
        var agentHandler = GetHandler(_aiHandler);
        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("ChatSample 开始运行（Microsoft Agent Framework + AgentSession）");
        Console.WriteLine("[聊天设置 1/2] 请输入 System Message，留空则使用默认：");
        Console.WriteLine("------ System Message Start ------");
        Console.WriteLine(Senparc.AI.DefaultSetting.DEFAULT_SYSTEM_MESSAGE);
        Console.WriteLine("------ System Message End ------");
        var systemMessage = Console.ReadLine();
        systemMessage = systemMessage.IsNullOrEmpty()
            ? Senparc.AI.DefaultSetting.DEFAULT_SYSTEM_MESSAGE
            : systemMessage;

        Console.WriteLine("[聊天设置 2/2] 会话模式：");
        Console.WriteLine("[1] 共享 AgentSession（同一 BuildKernel，保留上下文，推荐）");
        Console.WriteLine("[2] 每轮新建 Session（无上下文，用于对比测试）");
        var sessionMode = Console.ReadLine() == "2" ? SessionMode.PerRequest : SessionMode.Shared;

        Console.WriteLine("[聊天设置 3/3] 执行方式：");
        Console.WriteLine("[1] 严格流式 RunChatStreamingAsync（推荐；上游异常会原样抛出）");
        Console.WriteLine("[2] 严格单次 RunChatResponseAsync（上游异常会原样抛出）");
        Console.WriteLine("[3] 兼容 RunChatAsync（保留历史错误封装行为）");
        var executionMode = Console.ReadLine() switch
        {
            "2" => ChatExecutionMode.StrictResponse,
            "3" => ChatExecutionMode.Compatibility,
            _ => ChatExecutionMode.StrictStreaming
        };

        var chatOptions = new ChatClientAgentOptions
        {
            ChatOptions = new() 
            { 
                Instructions = systemMessage, 
                Temperature=0.2f // Senparc.AI 会自动忽略不被支持的参数（如使用 GPT-5.6 模型）
            }
        };

        Console.WriteLine();
        Console.WriteLine($"配置完成。当前执行方式：{GetExecutionModeDisplayName(executionMode)}。输入 exit 退出对话。");
        Console.WriteLine($"[调试] HttpClient 日志：{(SampleSetting.EnableHttpClientLog ? "开启" : "关闭")}；所有模式均使用 AgentKernelHelper 已配置的 HttpClient 传输链路。");
        Console.WriteLine("---------------------------------");

        var userId = "Jeffrey";
        var round = 0;

        IWantToRun? sharedRun = null;
        AgentSession? agentSession = null;

        if (sessionMode == SessionMode.Shared)
        {
            sharedRun = await agentHandler.IWantTo(SampleSetting.CurrentSetting)
                .ConfigChatModel(userId, chatOptions)
                .BuildKernelWithAgentSessionAsync();
            agentSession = sharedRun.Kernel.AgentSession;
            Console.WriteLine($"[调试] AgentSession 已创建：{agentSession != null}");
        }

        while (true)
        {
            Console.WriteLine($"[{round + 1}] 人类：");
            var input = Console.ReadLine();
            if (input.IsNullOrEmpty())
            {
                Console.WriteLine("[请输入有效内容]");
                continue;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            round++;
            Console.WriteLine($"[{round}] 机器：");

            try
            {
                IWantToRun iWantToRun;
                if (sessionMode == SessionMode.Shared)
                {
                    iWantToRun = sharedRun!;
                }
                else
                {
                    iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
                        .ConfigModel(ConfigModel.Chat, userId)
                        .BuildKernel(chatOptions);
                    agentSession = iWantToRun.Kernel.AgentSession;//实际为 null
                }

                switch (executionMode)
                {
                    case ChatExecutionMode.StrictStreaming:
                        await RunStrictStreamingAsync(iWantToRun, input, agentSession);
                        break;
                    case ChatExecutionMode.StrictResponse:
                        await RunStrictResponseAsync(iWantToRun, input, agentSession);
                        break;
                    case ChatExecutionMode.Compatibility:
                        await RunCompatibilityAsync(iWantToRun, input, agentSession);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(executionMode), executionMode, null);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"发生错误（{GetExecutionModeDisplayName(executionMode)}）：{ex.GetBaseException().Message}");
                if (executionMode != ChatExecutionMode.Compatibility)
                {
                    SampleHelper.PrintNote("严格模式不会将上游失败伪装为正常文本回复，适合 A2A、Workflow 和服务端调用方。");
                }
            }

            Console.WriteLine();
        }
    }

    private static async Task RunStrictStreamingAsync(IWantToRun iWantToRun, string input, AgentSession? agentSession)
    {
        UsageContent? usageContent = null;
        await foreach (var update in iWantToRun.RunChatStreamingAsync(input, agentSession))
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(update.Text);

            usageContent = update.Contents?
                .OfType<UsageContent>()
                .LastOrDefault() ?? usageContent;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        PrintUsage(usageContent?.Details);
    }

    private static async Task RunStrictResponseAsync(IWantToRun iWantToRun, string input, AgentSession? agentSession)
    {
        var response = await iWantToRun.RunChatResponseAsync(input, agentSession);
        Console.WriteLine(response?.Text ?? "[未收到文本回复]");
        PrintUsage(response?.Usage);
    }

    private static async Task RunCompatibilityAsync(IWantToRun iWantToRun, string input, AgentSession? agentSession)
    {
        Action<AgentResponseUpdate> updateFun = update =>
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(update.Text);
        };

        // 旧入口的参数标注未允许 null，但其默认值和实现均支持无 Session 的单轮执行。
        var result = await iWantToRun.RunChatAsync(input, agentSession!, updateFun);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();

        if (result.Result is null)
        {
            Console.WriteLine($"[兼容结果] {result.OutputString}");
            if (result.LastException is not null)
            {
                SampleHelper.PrintNote($"[兼容模式已记录异常] {result.LastException.GetBaseException().Message}");
            }

            return;
        }

        PrintUsage(result.Result.Usage);
    }

    private static void PrintUsage(UsageDetails? usage)
    {
        Console.WriteLine($"[调试] Tokens — input: {usage?.InputTokenCount}, output: {usage?.OutputTokenCount}, total: {usage?.TotalTokenCount}");
    }

    private static string GetExecutionModeDisplayName(ChatExecutionMode executionMode) => executionMode switch
    {
        ChatExecutionMode.StrictStreaming => "严格流式",
        ChatExecutionMode.StrictResponse => "严格单次响应",
        ChatExecutionMode.Compatibility => "兼容入口",
        _ => executionMode.ToString()
    };

    private enum SessionMode
    {
        Shared,
        PerRequest
    }

    private enum ChatExecutionMode
    {
        StrictStreaming,
        StrictResponse,
        Compatibility
    }
}
