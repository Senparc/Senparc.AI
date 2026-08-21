using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;

namespace Senparc.AI.AgentKernel.Tests.BaseSupport;

/// <summary>
/// Shared helper for RunChat / RunChatStreaming integration tests
/// </summary>
internal static class RunChatTestHelper
{
    public const string DefaultUserId = "Jeffrey";
    public const string ShortPrompt = "Introduce Suzhou gardens in one sentence.";
    public const string FollowUpPrompt = "Add one more attraction name.";

    public static AgentAiHandler CreateHandler(ISenparcAiSetting? setting = null)
        => new AgentAiHandler(setting);

    public static async Task<IWantToRun> BuildChatRun(AgentAiHandler? handler = null, string userId = DefaultUserId)
    {
        handler ??= CreateHandler();
        return await handler.IWantTo()
            .ConfigModel(ConfigModel.Chat, userId)
            .BuildKernelWithAgentSessionAsync();
    }

    public static async Task<IWantToRun> BuildChatRunWithSessionAsync(AgentAiHandler? handler = null, string userId = DefaultUserId)
    {
        handler ??= CreateHandler();
        return await handler.IWantTo()
            .ConfigModel(ConfigModel.Chat, userId)
            .BuildKernelWithAgentSessionAsync();
    }

    /// <summary>
    /// The NeuCharAI gateway usually does not support streaming; other platforms support it by default.
    /// </summary>
    public static bool SupportsStreaming(ISenparcAiSetting setting)
        => setting.AiPlatform != AiPlatform.NeuCharAI;

    public static void AssertAgentResponse(AgentResponse response, string? message = null)
    {
        Assert.IsNotNull(response, message);
        Assert.IsTrue(
            !string.IsNullOrWhiteSpace(response.Text) || (response.Usage?.TotalTokenCount ?? 0) > 0,
            message ?? "The response should contain text or token usage");
        if (response.CreatedAt.HasValue)
        {
            Assert.IsTrue(
                response.CreatedAt.Value.UtcDateTime > DateTime.UtcNow.AddMinutes(-10),
                message ?? "CreatedAt should be within the last 10 minutes");

            Console.WriteLine($"Usage - Input:{response.Usage.InputTokenCount} Output: {response.Usage.OutputTokenCount}");
        }
    }

    public static async Task<string> CollectStreamingTextAsync(IAsyncEnumerable<AgentResponseUpdate> stream)
    {
        var sb = new System.Text.StringBuilder();
        await foreach (var update in stream)
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                sb.Append(update.Text);
            }
        }

        return sb.ToString();
    }
}
