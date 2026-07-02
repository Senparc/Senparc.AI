using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Multi-turn conversation sample. See AgentAiHandlerTests.ConversationTestWithDefaultSession.
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

        throw new InvalidOperationException("This sample requires AgentAiHandler. Confirm that AddSenparcAI has been called.");
    }

    public async Task RunAsync()
    {
        SyncHttpClientLog();
        var agentHandler = GetHandler(_aiHandler);
        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("ChatSample started(Microsoft Agent Framework + AgentSession)");
        Console.WriteLine("[Chat settings 1/2] Enter the System Message. Leave it empty to use the default:");
        Console.WriteLine("------ System Message Start ------");
        Console.WriteLine(Senparc.AI.DefaultSetting.DEFAULT_SYSTEM_MESSAGE);
        Console.WriteLine("------ System Message End ------");
        var systemMessage = Console.ReadLine();
        systemMessage = systemMessage.IsNullOrEmpty()
            ? Senparc.AI.DefaultSetting.DEFAULT_SYSTEM_MESSAGE
            : systemMessage;

        Console.WriteLine("[Chat settings 2/2] Session mode:");
        Console.WriteLine("[1] Shared AgentSession (same BuildKernel, keeps context, recommended)");
        Console.WriteLine("[2] New Session each round (no context, for comparison testing)");
        var sessionMode = Console.ReadLine() == "2" ? SessionMode.PerRequest : SessionMode.Shared;

        var chatOptions = new ChatClientAgentOptions
        {
            ChatOptions = new() { Instructions = systemMessage }
        };

        Console.WriteLine();
        Console.WriteLine("Configuration complete. Enter exit to leave the conversation.");
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
            Console.WriteLine($"[Debug] AgentSession created:{agentSession != null}");
        }

        while (true)
        {
            Console.WriteLine($"[{round + 1}] Human:");
            var input = Console.ReadLine();
            if (input.IsNullOrEmpty())
            {
                Console.WriteLine("[Enter valid content]");
                continue;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            round++;
            Console.WriteLine($"[{round}] Assistant:");

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
                    agentSession = iWantToRun.Kernel.AgentSession;//Actually null
                }

                Action changeColor = () =>
                {
                    Console.ForegroundColor = Console.ForegroundColor == ConsoleColor.DarkYellow ? ConsoleColor.White : ConsoleColor.DarkYellow;
                };

                Action<AgentResponseUpdate> updateFun = update =>
                {
                    changeColor();
                    Console.Write(update.Text);
                };

                var result = await iWantToRun.RunChatAsync(input, agentSession, updateFun);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.WriteLine($"[Debug] Tokens — input: {result.Result.Usage?.InputTokenCount}, output: {result.Result.Usage?.OutputTokenCount}, total: {result.Result.Usage?.TotalTokenCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:" + ex);
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
