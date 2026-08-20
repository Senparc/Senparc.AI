/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    File: ChatSample.cs
    Description: Demonstrates configuring and interacting with an AgentKernel-based multi-turn chat session.


    Created by: Senparc - 20260521

    Modified by: Senparc - 20260813
    Change description: v1.3.0 added strict IWantToRun execution mode examples.

----------------------------------------------------------------*/
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Multi-turn chat example based on AgentAiHandlerTests.ConversationTestWithDefaultSession.
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

        Console.WriteLine("ChatSample started (Microsoft Agent Framework + AgentSession)");
        Console.WriteLine("[Chat setting 1/2] Enter the System Message, or leave it blank to use the default:");
        Console.WriteLine("------ System Message Start ------");
        Console.WriteLine(Senparc.AI.DefaultSetting.DEFAULT_SYSTEM_MESSAGE);
        Console.WriteLine("------ System Message End ------");
        var systemMessage = Console.ReadLine();
        systemMessage = systemMessage.IsNullOrEmpty()
            ? Senparc.AI.DefaultSetting.DEFAULT_SYSTEM_MESSAGE
            : systemMessage;

        Console.WriteLine("[Chat setting 2/2] Session mode:");
        Console.WriteLine("[1] Shared AgentSession (same BuildKernel, retains context, recommended)");
        Console.WriteLine("[2] New Session for each turn (no context, for comparison testing)");
        var sessionMode = Console.ReadLine() == "2" ? SessionMode.PerRequest : SessionMode.Shared;

        Console.WriteLine("[Chat setting 3/3] Execution mode:");
        Console.WriteLine("[1] Strict streaming RunChatStreamingAsync (recommended; upstream exceptions are rethrown unchanged)");
        Console.WriteLine("[2] Strict single-response RunChatResponseAsync (upstream exceptions are rethrown unchanged)");
        Console.WriteLine("[3] Compatible RunChatAsync (preserves legacy error-wrapping behavior)");
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
                Temperature=0.2f // Senparc.AI automatically ignores unsupported parameters, such as when using a GPT-5.6 model.
            }
        };

        Console.WriteLine();
        Console.WriteLine($"Configuration complete. Current execution mode: {GetExecutionModeDisplayName(executionMode)}. Enter exit to leave the chat.");
        Console.WriteLine($"[Debug] HttpClient logging: {(SampleSetting.EnableHttpClientLog ? "enabled" : "disabled")}; all modes use the HttpClient transport chain configured by AgentKernelHelper.");
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
            Console.WriteLine($"[Debug] AgentSession created: {agentSession != null}");
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
                    agentSession = iWantToRun.Kernel.AgentSession;// Actually null.
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
                Console.WriteLine($"An error occurred ({GetExecutionModeDisplayName(executionMode)}): {ex.GetBaseException().Message}");
                if (executionMode != ChatExecutionMode.Compatibility)
                {
                    SampleHelper.PrintNote("Strict modes do not disguise upstream failures as normal text responses, making them suitable for A2A, Workflows, and server-side callers.");
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
        Console.WriteLine(response?.Text ?? "[No text response received]");
        PrintUsage(response?.Usage);
    }

    private static async Task RunCompatibilityAsync(IWantToRun iWantToRun, string input, AgentSession? agentSession)
    {
        Action<AgentResponseUpdate> updateFun = update =>
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(update.Text);
        };

        // The legacy entry point does not annotate this parameter as nullable, but its default value and implementation support single-turn execution without a Session.
        var result = await iWantToRun.RunChatAsync(input, agentSession!, updateFun);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();

        if (result.Result is null)
        {
            Console.WriteLine($"[Compatibility result] {result.OutputString}");
            if (result.LastException is not null)
            {
                SampleHelper.PrintNote($"[Exception recorded by compatibility mode] {result.LastException.GetBaseException().Message}");
            }

            return;
        }

        PrintUsage(result.Result.Usage);
    }

    private static void PrintUsage(UsageDetails? usage)
    {
        Console.WriteLine($"[Debug] Tokens - input: {usage?.InputTokenCount}, output: {usage?.OutputTokenCount}, total: {usage?.TotalTokenCount}");
    }

    private static string GetExecutionModeDisplayName(ChatExecutionMode executionMode) => executionMode switch
    {
        ChatExecutionMode.StrictStreaming => "strict streaming",
        ChatExecutionMode.StrictResponse => "strict single response",
        ChatExecutionMode.Compatibility => "compatibility entry point",
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
