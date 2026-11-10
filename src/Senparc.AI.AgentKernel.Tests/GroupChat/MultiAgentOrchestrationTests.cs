using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Senparc.AI.AgentKernel.Tests.GroupChat;

/// <summary>
/// Offline coverage for MAF 1.28 orchestration. These tests use a scripted chat client
/// so group chat, sequential, and concurrent workflows can run without a model endpoint.
/// </summary>
[TestClass]
public class MultiAgentOrchestrationTests
{
    [TestMethod]
    public async Task GroupChat_BroadcastsOnlyTheCurrentTurn()
    {
        var researcher = ScriptedAgent("Researcher", "research-note");
        var writer = ScriptedAgent("Writer", "writer-done");
        RecordingRoundRobinManager? manager = null;

        var workflow = AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents =>
            {
                manager = new RecordingRoundRobinManager(
                    agents,
                    (_, history, _) => new ValueTask<bool>(
                        history.Any(message => (message.Text ?? string.Empty).Contains("writer-done", StringComparison.Ordinal))));
                manager.MaximumIterationCount = 6;
                return manager;
            })
            .AddParticipants([researcher, writer])
            .WithName("OfflineGroupChat")
            .Build();

        var events = await RunAsync(workflow, "Draft a short note.");

        Assert.IsNotNull(manager);
        Assert.IsTrue(manager!.BroadcastCounts.Count >= 2, "Group chat should broadcast the user turn and at least one agent turn.");
        Assert.IsTrue(manager.BroadcastCounts.All(count => count == 1),
            "MAF 1.28 UpdateHistoryAsync receives only the current broadcast, not the accumulated history. Counts: "
            + string.Join(",", manager.BroadcastCounts));
        Assert.IsTrue(events.OfType<WorkflowErrorEvent>().Any() == false, FormatErrors(events));
        Assert.IsTrue(ContainsText(events, "research-note"));
        Assert.IsTrue(ContainsText(events, "writer-done"));
    }

    [TestMethod]
    public async Task Sequential_EmitsTheLastParticipant()
    {
        var workflow = AgentWorkflowBuilder
            .CreateSequentialBuilderWith([
                ScriptedAgent("Outliner", "outline"),
                ScriptedAgent("Drafter", "draft"),
                ScriptedAgent("Editor", "final-copy")
            ])
            .WithName("OfflineSequential")
            .Build();

        var events = await RunAsync(workflow, "Write a paragraph.");

        Assert.IsFalse(events.OfType<WorkflowErrorEvent>().Any(), FormatErrors(events));
        Assert.IsTrue(ContainsText(events, "final-copy"));
    }

    [TestMethod]
    public async Task Concurrent_CollectsEveryParticipant()
    {
        var workflow = AgentWorkflowBuilder
            .CreateConcurrentBuilderWith([
                ScriptedAgent("Facts", "fact-result"),
                ScriptedAgent("Risks", "risk-result")
            ])
            .WithName("OfflineConcurrent")
            .Build();

        var events = await RunAsync(workflow, "Review this change.");

        Assert.IsFalse(events.OfType<WorkflowErrorEvent>().Any(), FormatErrors(events));
        Assert.IsTrue(ContainsText(events, "fact-result"));
        Assert.IsTrue(ContainsText(events, "risk-result"));
    }

    [TestMethod]
    public async Task Handoff_BuildsAStableWorkflow()
    {
        var triage = ScriptedAgent("Triage", "triage-ack");
        var billing = ScriptedAgent("Billing", "billing-ack");
        var workflow = AgentWorkflowBuilder
            .CreateHandoffBuilderWith(triage)
            .WithHandoff(triage, billing, "Hand off billing questions.")
            .WithTerminationCondition(history => history.Any(message => (message.Text ?? string.Empty).Contains("ack", StringComparison.Ordinal)))
            .WithName("OfflineHandoff")
            .Build();

        var events = await RunAsync(workflow, "I need an invoice.");

        Assert.IsNotNull(workflow);
        Assert.IsFalse(events.OfType<WorkflowErrorEvent>().Any(), FormatErrors(events));
        Assert.IsTrue(ContainsText(events, "triage-ack") || ContainsText(events, "billing-ack"));
    }

    private static AIAgent ScriptedAgent(string name, string reply)
    {
        return new ScriptedChatClient(reply).AsAIAgent(new ChatClientAgentOptions
        {
            Name = name,
            Description = name,
            ChatOptions = new ChatOptions
            {
                Instructions = $"You are {name}."
            }
        });
    }

    private static async Task<List<WorkflowEvent>> RunAsync(Workflow workflow, string prompt)
    {
        var events = new List<WorkflowEvent>();
        await using var run = await InProcessExecution.RunStreamingAsync(
            workflow,
            new List<ChatMessage> { new(ChatRole.User, prompt) });
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        await foreach (var workflowEvent in run.WatchStreamAsync())
        {
            events.Add(workflowEvent);
        }

        return events;
    }

    private static bool ContainsText(IEnumerable<WorkflowEvent> events, string text)
    {
        foreach (var workflowEvent in events)
        {
            switch (workflowEvent)
            {
                case AgentResponseUpdateEvent update when (update.Update.Text ?? string.Empty).Contains(text, StringComparison.Ordinal):
                    return true;
                case WorkflowOutputEvent output when OutputText(output).Contains(text, StringComparison.Ordinal):
                    return true;
            }
        }

        return false;
    }

    private static string OutputText(WorkflowOutputEvent output)
    {
        if (output.Data is List<ChatMessage> messages)
        {
            return string.Join("\n", messages.Select(message => message.Text));
        }

        if (output.Data is ChatMessage message)
        {
            return message.Text ?? string.Empty;
        }

        return output.Data?.ToString() ?? string.Empty;
    }

    private static string FormatErrors(IEnumerable<WorkflowEvent> events)
    {
        var builder = new StringBuilder();
        foreach (var error in events.OfType<WorkflowErrorEvent>())
        {
            builder.AppendLine(error.Exception?.ToString() ?? "Unknown workflow error.");
        }

        foreach (var failed in events.OfType<ExecutorFailedEvent>())
        {
            builder.AppendLine($"{failed.ExecutorId}: {failed.Data}");
        }

        return builder.ToString();
    }

    private sealed class RecordingRoundRobinManager : RoundRobinGroupChatManager
    {
        public List<int> BroadcastCounts { get; } = [];

        public RecordingRoundRobinManager(
            IReadOnlyList<AIAgent> agents,
            Func<RoundRobinGroupChatManager, IEnumerable<ChatMessage>, CancellationToken, ValueTask<bool>> shouldTerminateFunc)
            : base(agents, shouldTerminateFunc)
        {
        }

        protected override ValueTask<IEnumerable<ChatMessage>> UpdateHistoryAsync(
            IReadOnlyList<ChatMessage> history,
            CancellationToken cancellationToken = default)
        {
            BroadcastCounts.Add(history.Count);
            return base.UpdateHistoryAsync(history, cancellationToken);
        }
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
