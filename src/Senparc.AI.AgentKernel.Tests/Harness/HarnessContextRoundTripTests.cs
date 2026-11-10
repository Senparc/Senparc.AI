using System.Runtime.CompilerServices;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Harness;

namespace Senparc.AI.AgentKernel.Tests.Harness;

#pragma warning disable MAAI001

/// <summary>
/// Guards the MAF 1.28 compaction interaction that drops a later user turn.
/// </summary>
[TestClass]
public class HarnessContextRoundTripTests
{
    [TestMethod]
    public async Task Compaction_KeepsTheNewUserMessageWhenTheTodoStatusRepeats()
    {
        var client = new RecordingChatClient();
        var options = new HarnessAgentOptions
        {
            Name = "ContextProbe",
            MaxContextWindowTokens = 32_768,
            MaxOutputTokens = 2_048,
            ChatOptions = new ChatOptions
            {
                Instructions = "You are a rigorous assistant."
            },
            DisableFileMemory = true,
            DisableAgentSkillsProvider = true,
            DisableWebSearch = true
        };
        options.UseCompactionSafeTodoList();

        AIAgent agent = client.AsHarnessAgent(options);
        var session = await agent.CreateSessionAsync();
        Assert.IsNotNull(agent.GetService<TodoProvider>());

        await agent.RunAsync([new ChatMessage(ChatRole.User, "你好")], session);
        await agent.RunAsync([new ChatMessage(ChatRole.User, "你是什么模型?")], session);

        var latest = client.Requests[^1].Messages;
        Assert.IsTrue(latest.Any(message => (message.Text ?? string.Empty).Contains("你好", StringComparison.Ordinal)));
        Assert.IsTrue(latest.Any(message => (message.Text ?? string.Empty).Contains("你是什么模型?", StringComparison.Ordinal)));
    }

    private sealed class RecordingChatClient : IChatClient
    {
        public List<RecordedRequest> Requests { get; } = [];

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var copy = messages.ToList();
            Requests.Add(new RecordedRequest(copy, options));
            var lastUser = copy.LastOrDefault(message => message.Role == ChatRole.User)?.Text ?? "<none>";
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, $"echo:{lastUser}")));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var response = await GetResponseAsync(messages, options, cancellationToken);
            yield return new ChatResponseUpdate(ChatRole.Assistant, response.Text);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) =>
            serviceType.IsInstanceOfType(this) ? this : null;

        public void Dispose()
        {
        }
    }

    private sealed record RecordedRequest(List<ChatMessage> Messages, ChatOptions? Options);
}
