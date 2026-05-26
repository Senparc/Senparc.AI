using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;

namespace Senparc.AI.AgentKernel.Tests.BaseSupport;

/// <summary>
/// RunChat / RunChatStreaming 集成测试共用辅助
/// </summary>
internal static class RunChatTestHelper
{
    public const string DefaultUserId = "Jeffrey";
    public const string ShortPrompt = "用一句话介绍苏州园林。";
    public const string FollowUpPrompt = "再补充一个景点名称。";

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
    /// NeuCharAI 网关通常不支持流式；其他平台默认支持。
    /// </summary>
    public static bool SupportsStreaming(ISenparcAiSetting setting)
        => setting.AiPlatform != AiPlatform.NeuCharAI;

    public static void AssertAgentResponse(AgentResponse response, string? message = null)
    {
        Assert.IsNotNull(response, message);
        Assert.IsTrue(
            !string.IsNullOrWhiteSpace(response.Text) || (response.Usage?.TotalTokenCount ?? 0) > 0,
            message ?? "响应应包含文本或 Token 用量");
        if (response.CreatedAt.HasValue)
        {
            Assert.IsTrue(
                response.CreatedAt.Value.UtcDateTime > DateTime.UtcNow.AddMinutes(-10),
                message ?? "CreatedAt 应在最近 10 分钟内");

            Console.WriteLine($"用量 - Input：{response.Usage.InputTokenCount} Output: {response.Usage.OutputTokenCount}");
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
