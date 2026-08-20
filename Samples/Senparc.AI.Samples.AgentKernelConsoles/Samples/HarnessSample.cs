/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    File: HarnessSample.cs
    Description: Demonstrates creating a Microsoft Agent Framework HarnessAgent and interacting with it from the command line.


    Created by: Senparc - 20260808

----------------------------------------------------------------*/
#pragma warning disable MAAI001 // Some APIs in Microsoft.Agents.AI.Harness 1.8 are still marked as experimental.

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using OpenAIChatClient = OpenAI.Chat.ChatClient;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// HarnessAgent command-line example.
/// </summary>
public class HarnessSample
{
    private const int DefaultMaxContextWindowTokens = 32_768;
    private const int DefaultMaxOutputTokens = 2_048;

    private readonly IAiHandler _aiHandler;

    public HarnessSample(IAiHandler aiHandler)
    {
        _aiHandler = aiHandler;
    }

    public async Task RunAsync()
    {
        var agentHandler = GetHandler(_aiHandler);
        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("HarnessSample started (Microsoft Agent Framework HarnessAgent)");
        Console.WriteLine("The exact extension method is IChatClient.AsHarnessAgent(...). This sample disables file, Skills, and Hosted Web Search permissions by default.");
        Console.WriteLine();

        Console.WriteLine("Enter Agent-specific instructions, or leave the input blank to use the default:");
        const string defaultInstructions = "You are a rigorous assistant. Plan complex tasks first, then execute them step by step and report the results clearly.";
        Console.WriteLine($"Default: {defaultInstructions}");
        var instructions = Console.ReadLine();
        instructions = instructions.IsNullOrEmpty() ? defaultInstructions : instructions;

        var maxContextWindowTokens = ReadPositiveInt(
            $"Model context-window Token limit (default {DefaultMaxContextWindowTokens}):",
            DefaultMaxContextWindowTokens);
        var maxOutputTokens = ReadPositiveInt(
            $"Per-response model Token limit (default {DefaultMaxOutputTokens}):",
            DefaultMaxOutputTokens);

        if (maxOutputTokens >= maxContextWindowTokens)
        {
            Console.WriteLine("The output Token limit must be lower than the context-window limit. Returning to the main menu.");
            return;
        }

        var iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigModel(ConfigModel.Chat, "HarnessSample")
            .BuildKernel();

        var chatClient = iWantToRun.Kernel.ChatClient switch
        {
            OpenAIChatClient client => client.AsIChatClient(),
            IChatClient client => client,
            var client => throw new NotSupportedException(
                $"HarnessSample does not currently support ChatClient type: {client?.GetType().FullName ?? "null"}")
        };

        AIAgent agent = chatClient.AsHarnessAgent(
            maxContextWindowTokens,
            maxOutputTokens,
            new HarnessAgentOptions
            {
                Name = "SenparcAgentKernelHarness",
                Description = "Senparc.AI AgentKernel Harness command-line sample",
                ChatOptions = new ChatOptions
                {
                    Instructions = instructions
                },

                // Harness 1.8 creates local directories or adds model-specific tools for the following capabilities by default.
                // The sample uses least-privilege defaults; enable them explicitly in a controlled directory when needed.
                DisableFileAccess = true,
                DisableFileMemory = true,
                DisableAgentSkillsProvider = true,
                DisableWebSearch = true
            });

        var session = await agent.CreateSessionAsync();
        var modeProvider = agent.GetService<AgentModeProvider>();
        var todoProvider = agent.GetService<TodoProvider>();

        Console.WriteLine();
        Console.WriteLine("Harness started. Available commands:");
        Console.WriteLine("  /mode                 View the current mode");
        Console.WriteLine("  /mode plan|execute    Switch between plan and execute modes");
        Console.WriteLine("  /todos                View the Harness Todo list");
        Console.WriteLine("  exit                  Return to the main menu");
        Console.WriteLine("When tool execution requires permission, the command line requests confirmation for each item.");

        while (true)
        {
            Console.WriteLine();
            Console.Write("Human: ");
            var input = Console.ReadLine();

            if (input is null || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (input.IsNullOrEmpty())
            {
                continue;
            }

            if (await TryHandleCommandAsync(input, session, modeProvider, todoProvider))
            {
                continue;
            }

            Console.Write("Assistant: ");
            try
            {
                await RunAgentTurnAsync(agent, session, [new ChatMessage(ChatRole.User, input)]);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"An error occurred: {ex}");
            }
        }
    }

    private static AgentAiHandler GetHandler(IAiHandler handler)
    {
        return handler as AgentAiHandler
            ?? throw new InvalidOperationException("This sample requires AgentAiHandler. Confirm that AddSenparcAI has been called.");
    }

    private static int ReadPositiveInt(string prompt, int defaultValue)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        if (input.IsNullOrEmpty())
        {
            return defaultValue;
        }

        if (int.TryParse(input, out var value) && value > 0)
        {
            return value;
        }

        Console.WriteLine($"Invalid input; using the default value {defaultValue}.");
        return defaultValue;
    }

    private static async Task<bool> TryHandleCommandAsync(
        string input,
        AgentSession session,
        AgentModeProvider? modeProvider,
        TodoProvider? todoProvider)
    {
        if (input.Equals("/todos", StringComparison.OrdinalIgnoreCase))
        {
            if (todoProvider is null)
            {
                Console.WriteLine("TodoProvider is unavailable.");
                return true;
            }

            var todos = await todoProvider.GetAllTodosAsync(session);
            if (todos.Count == 0)
            {
                Console.WriteLine("There are currently no Todos.");
                return true;
            }

            Console.WriteLine("Todo list:");
            foreach (var item in todos)
            {
                Console.WriteLine($"  [{(item.IsComplete ? "x" : " ")}] #{item.Id} {item.Title} {item.Description}");
            }

            return true;
        }

        if (!input.Equals("/mode", StringComparison.OrdinalIgnoreCase)
            && !input.StartsWith("/mode ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (modeProvider is null)
        {
            Console.WriteLine("AgentModeProvider is unavailable.");
            return true;
        }

        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            Console.WriteLine($"Current mode: {modeProvider.GetMode(session)}");
            return true;
        }

        try
        {
            modeProvider.SetMode(session, parts[1]);
            Console.WriteLine($"Switched to {parts[1]} mode.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return true;
    }

    private static async Task RunAgentTurnAsync(
        AIAgent agent,
        AgentSession session,
        IList<ChatMessage> messages)
    {
        IList<ChatMessage>? nextMessages = messages;

        while (nextMessages is not null)
        {
            var approvalRequests = new List<ToolApprovalRequestContent>();

            await foreach (var update in agent.RunStreamingAsync(nextMessages, session))
            {
                if (!update.Text.IsNullOrEmpty())
                {
                    Console.Write(update.Text);
                }

                foreach (var content in update.Contents)
                {
                    if (content is ToolApprovalRequestContent request)
                    {
                        approvalRequests.Add(request);
                    }
                }
            }

            Console.WriteLine();

            if (approvalRequests.Count == 0)
            {
                nextMessages = null;
                continue;
            }

            var responses = new List<ChatMessage>(approvalRequests.Count);
            foreach (var request in approvalRequests)
            {
                var toolName = request.ToolCall is FunctionCallContent functionCall
                    ? functionCall.Name
                    : request.ToolCall.GetType().Name;

                Console.Write($"Tool {toolName} requests execution. Allow it? [y/N] ");
                var approved = Console.ReadLine()?.Equals("y", StringComparison.OrdinalIgnoreCase) == true;
                var response = request.CreateResponse(
                    approved,
                    approved ? "The user approved execution from the command line" : "The user denied execution");
                responses.Add(new ChatMessage(ChatRole.User, [response]));
            }

            nextMessages = responses;
        }
    }
}
