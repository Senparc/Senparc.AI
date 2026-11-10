/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    File: MultiAgentOrchestrationSample.cs
    Description: Demonstrates MAF 1.28 multi-agent orchestration through AgentKernel.


    Created by: Senparc - 20261111

----------------------------------------------------------------*/

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using System.Runtime.CompilerServices;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Shows the stable MAF 1.28 orchestration builders: group chat, sequential, concurrent, handoff, and Magentic.
/// Offline mode uses a scripted chat client so the sample can be executed without an API key.
/// </summary>
public class MultiAgentOrchestrationSample
{
    private readonly IAiHandler _aiHandler;

    public MultiAgentOrchestrationSample(IAiHandler aiHandler)
    {
        _aiHandler = aiHandler;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("MAF 1.28 multi-agent orchestration");
        Console.WriteLine("[1] Offline group chat, sequential, and concurrent (no model call)");
        Console.WriteLine("[2] Live group chat with the current Chat model");
        Console.WriteLine("[3] Live sequential pipeline");
        Console.WriteLine("[4] Live concurrent fan-out");
        Console.WriteLine("[5] Live handoff");
        Console.WriteLine("[6] Live Magentic");
        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                await RunOfflineAsync();
                break;
            case "2":
                await RunLiveGroupChatAsync();
                break;
            case "3":
                await RunLiveSequentialAsync();
                break;
            case "4":
                await RunLiveConcurrentAsync();
                break;
            case "5":
                await RunLiveHandoffAsync();
                break;
            case "6":
                await RunLiveMagenticAsync();
                break;
            default:
                Console.WriteLine("Invalid number.");
                break;
        }
    }

    public static async Task RunOfflineAsync()
    {
        Console.WriteLine("Offline orchestration started. Each participant is a scripted IChatClient.");

        var group = AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents =>
            {
                var manager = new RoundRobinGroupChatManager(
                    agents,
                    (_, history, _) => new ValueTask<bool>(
                        history.Any(message => (message.Text ?? string.Empty).Contains("writer-done", StringComparison.Ordinal))));
                manager.MaximumIterationCount = 4;
                return manager;
            })
            .AddParticipants([
                ScriptedAgent("Researcher", "research-note"),
                ScriptedAgent("Writer", "writer-done")
            ])
            .WithName("SampleGroupChat")
            .Build();
        await PrintRunAsync("GroupChat", group, "Summarize the topic.");

        var sequential = AgentWorkflowBuilder
            .CreateSequentialBuilderWith([
                ScriptedAgent("Outliner", "outline"),
                ScriptedAgent("Editor", "final-copy")
            ])
            .WithName("SampleSequential")
            .Build();
        await PrintRunAsync("Sequential", sequential, "Write a paragraph.");

        var concurrent = AgentWorkflowBuilder
            .CreateConcurrentBuilderWith([
                ScriptedAgent("Facts", "fact-result"),
                ScriptedAgent("Risks", "risk-result")
            ])
            .WithName("SampleConcurrent")
            .Build();
        await PrintRunAsync("Concurrent", concurrent, "Review this change.");

        Console.WriteLine("Offline orchestration completed.");
    }

    private async Task RunLiveGroupChatAsync()
    {
        var prompt = ReadPrompt("Ask the group to produce a two-step answer.");
        var host = await CreateAgentAsync("Host", "Decide who speaks next and finish when the answer is complete.");
        var researcher = await CreateAgentAsync("Researcher", "Give one factual point and stop.");
        var writer = await CreateAgentAsync("Writer", "Turn the previous point into one short sentence and stop.");
        var workflow = AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents =>
            {
                var manager = new RoundRobinGroupChatManager(agents);
                manager.MaximumIterationCount = 4;
                return manager;
            })
            .AddParticipants([host, researcher, writer])
            .WithName("LiveGroupChat")
            .Build();
        Console.WriteLine(workflow.ToMermaidString());
        await PrintRunAsync("LiveGroupChat", workflow, prompt);
    }

    private async Task RunLiveSequentialAsync()
    {
        var prompt = ReadPrompt("Give the pipeline one task.");
        var workflow = AgentWorkflowBuilder
            .CreateSequentialBuilderWith([
                await CreateAgentAsync("Outliner", "Produce a three-bullet outline."),
                await CreateAgentAsync("Editor", "Rewrite the outline as one paragraph.")
            ])
            .WithName("LiveSequential")
            .Build();
        await PrintRunAsync("LiveSequential", workflow, prompt);
    }

    private async Task RunLiveConcurrentAsync()
    {
        var prompt = ReadPrompt("Give both agents the same question.");
        var workflow = AgentWorkflowBuilder
            .CreateConcurrentBuilderWith([
                await CreateAgentAsync("Facts", "Answer with facts only."),
                await CreateAgentAsync("Risks", "Answer with risks only.")
            ])
            .WithName("LiveConcurrent")
            .Build();
        await PrintRunAsync("LiveConcurrent", workflow, prompt);
    }

    private async Task RunLiveHandoffAsync()
    {
        var prompt = ReadPrompt("Ask a billing question so triage can hand off.");
        var triage = await CreateAgentAsync("Triage", "If the user asks about billing, call the handoff tool. Otherwise answer directly.");
        var billing = await CreateAgentAsync("Billing", "Answer billing questions in one short paragraph.");
        var workflow = AgentWorkflowBuilder
            .CreateHandoffBuilderWith(triage)
            .WithHandoff(triage, billing, "Hand off billing questions.")
            .WithName("LiveHandoff")
            .Build();
        await PrintRunAsync("LiveHandoff", workflow, prompt);
    }

    private async Task RunLiveMagenticAsync()
    {
        var prompt = ReadPrompt("Ask the team to solve a small task.");
        var manager = await CreateAgentAsync("MagenticManager", "Coordinate the team, keep the plan short, and stop when the answer is ready.");
        var worker = await CreateAgentAsync("Worker", "Do the requested task and return only the result.");
        var workflow = AgentWorkflowBuilder
            .CreateMagenticBuilderWith(manager)
            .AddParticipants([worker])
            .RequirePlanSignoff(false)
            .WithMaxRounds(4)
            .WithName("LiveMagentic")
            .Build();
        await PrintRunAsync("LiveMagentic", workflow, prompt);
    }

    private async Task<AIAgent> CreateAgentAsync(string name, string instructions)
    {
        var handler = _aiHandler as AgentAiHandler
            ?? throw new InvalidOperationException("Multi-agent samples require AgentAiHandler.");
        var run = await handler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigChatModel(name, new ChatClientAgentOptions
            {
                Name = name,
                Description = instructions,
                ChatOptions = new ChatOptions
                {
                    Instructions = instructions,
                    MaxOutputTokens = 400
                }
            })
            .BuildKernelWithAgentSessionAsync();
        return run.Kernel.ChatClientAgent
            ?? throw new InvalidOperationException($"Chat model '{name}' did not create an agent.");
    }

    private static string ReadPrompt(string label)
    {
        Console.WriteLine(label);
        var prompt = Console.ReadLine();
        return string.IsNullOrWhiteSpace(prompt) ? label : prompt.Trim();
    }

    private static async Task PrintRunAsync(string title, Workflow workflow, string prompt)
    {
        Console.WriteLine();
        Console.WriteLine($"----- {title} -----");
        await using var run = await InProcessExecution.RunStreamingAsync(
            workflow,
            new List<ChatMessage> { new(ChatRole.User, prompt) });
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        await foreach (var workflowEvent in run.WatchStreamAsync())
        {
            switch (workflowEvent)
            {
                case AgentResponseUpdateEvent update:
                    Console.Write(update.Update.Text);
                    break;
                case WorkflowOutputEvent output:
                    Console.WriteLine();
                    Console.WriteLine($"[output] {Describe(output)}");
                    break;
                case WorkflowErrorEvent error:
                    Console.WriteLine();
                    Console.WriteLine("[error] " + error.Exception);
                    break;
                case ExecutorFailedEvent failed:
                    Console.WriteLine();
                    Console.WriteLine($"[executor-failed] {failed.ExecutorId}: {failed.Data}");
                    break;
            }
        }

        Console.WriteLine();
    }

    private static string Describe(WorkflowOutputEvent output)
    {
        if (output.Data is List<ChatMessage> messages)
        {
            return string.Join(" | ", messages.Select(message => $"{message.AuthorName ?? message.Role.ToString()}: {message.Text}"));
        }

        return output.Data?.ToString() ?? string.Empty;
    }

    private static AIAgent ScriptedAgent(string name, string reply)
    {
        return new ScriptedChatClient(reply).AsAIAgent(new ChatClientAgentOptions
        {
            Name = name,
            Description = name,
            ChatOptions = new ChatOptions { Instructions = $"You are {name}." }
        });
    }

    private sealed class ScriptedChatClient(string reply) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, reply)));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new ChatResponseUpdate(ChatRole.Assistant, reply);
            await Task.CompletedTask;
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return serviceType.IsInstanceOfType(this) ? this : null;
        }

        public void Dispose()
        {
        }
    }
}
