using Microsoft.Agents.AI;
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

        var chatOptions = new ChatClientAgentOptions
        {
            ChatOptions = new() { Instructions = systemMessage }
        };

        Console.WriteLine();
        Console.WriteLine("配置完成。输入 exit 退出对话。");
        Console.WriteLine("---------------------------------");

        var userId = "Jeffrey";
        var round = 0;

        IWantToRun? sharedRun = null;
        AgentSession? agentSession = null;

        if (sessionMode == SessionMode.Shared)
        {
            sharedRun = await agentHandler.IWantTo(SampleSetting.CurrentSetting)
                .ConfigModel(ConfigModel.Chat, userId)
                .BuildKernelWithAgentSessionAsync(chatOptions);
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

                var result = await iWantToRun.RunAsync(input, agentSession);
                Console.WriteLine(result.Result.Text);
                Console.WriteLine($"[调试] Tokens — input: {result.Result.Usage?.InputTokenCount}, output: {result.Result.Usage?.OutputTokenCount}, total: {result.Result.Usage?.TotalTokenCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生错误：" + ex);
            }

            Console.WriteLine();
        }
    }

    private enum SessionMode
    {
        Shared,
        PerRequest
    }
}
