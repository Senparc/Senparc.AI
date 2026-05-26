using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.AgentKernel.Tests.BaseSupport;

namespace Senparc.AI.AgentKernel.Tests.Kernels;

[TestClass]
public class AiKernelRunChatTests : KernelTestBase
{
    private AiKernel _kernel = null!;

    [TestInitialize]
    public void Init()
    {
        _kernel = RunChatTestHelper.BuildChatRun().GetAwaiter().GetResult().Kernel;
    }

    //[TestMethod]
    //public async Task RunChatAsync_StringPrompt_ReturnsResponse()
    //{
    //    var response = await _kernel.RunChatAsync(RunChatTestHelper.ShortPrompt);

    //    RunChatTestHelper.AssertAgentResponse(response);
    //    Console.WriteLine($"[RunChatAsync] {response.Text}");
    //}

    //[TestMethod]
    //public async Task RunChatAsync_GenericString_ReturnsTypedResponse()
    //{
    //    var response = await _kernel.RunChatAsync<string>(RunChatTestHelper.ShortPrompt);

    //    RunChatTestHelper.AssertAgentResponse(response);
    //    Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text));
    //}

    //[TestMethod]
    //public async Task RunChatAsync_ChatMessages_ReturnsResponse()
    //{
    //    var messages = new[]
    //    {
    //        new ChatMessage(ChatRole.User, RunChatTestHelper.ShortPrompt)
    //    };

    //    var response = await _kernel.RunChatAsync(messages);

    //    RunChatTestHelper.AssertAgentResponse(response);
    //}

    //[TestMethod]
    //public async Task RunChatAsync_SingleChatMessage_ReturnsResponse()
    //{
    //    var message = new ChatMessage(ChatRole.User, RunChatTestHelper.ShortPrompt);
    //    var response = await _kernel.RunChatAsync(new[] { message });

    //    RunChatTestHelper.AssertAgentResponse(response);
    //}

    //[TestMethod]
    //public async Task RunChatAsync_WithAgentSession_KeepsContext()
    //{
    //    var iWantToRun = await RunChatTestHelper.BuildChatRunWithSessionAsync();
    //    var session = iWantToRun.Kernel.AgentSession;
    //    Assert.IsNotNull(session);

    //    var first = await iWantToRun.Kernel.RunChatAsync("苏州特产有哪些？请只说三个。", session);
    //    var second = await iWantToRun.Kernel.RunChatAsync(RunChatTestHelper.FollowUpPrompt, session);

    //    RunChatTestHelper.AssertAgentResponse(first);
    //    RunChatTestHelper.AssertAgentResponse(second);
    //    Console.WriteLine($"[Session] 1: {first.Text}");
    //    Console.WriteLine($"[Session] 2: {second.Text}");
    //}

    //[TestMethod]
    //public async Task RunChatStreamingAsync_StringPrompt_YieldsUpdates()
    //{
    //    if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
    //    {
    //        Assert.Inconclusive("当前 AiPlatform 不支持流式输出，已跳过。");
    //        return;
    //    }

    //    var updates = new List<AgentResponseUpdate>();
    //    await foreach (var update in _kernel.RunChatStreamingAsync(RunChatTestHelper.ShortPrompt))
    //    {
    //        updates.Add(update);
    //    }

    //    Assert.IsTrue(updates.Count > 0, "流式应至少返回一个 AgentResponseUpdate");
    //    var text = updates.ToAgentResponse().Text ?? string.Empty;
    //    Assert.IsFalse(string.IsNullOrWhiteSpace(text), "流式聚合文本不应为空");
    //    Console.WriteLine($"[RunChatStreamingAsync] chunks={updates.Count}, text={text}");
    //}

    //[TestMethod]
    //public async Task RunChatStreamingAsync_ChatMessages_YieldsUpdates()
    //{
    //    if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
    //    {
    //        Assert.Inconclusive("当前 AiPlatform 不支持流式输出，已跳过。");
    //        return;
    //    }

    //    var messages = new[] { new ChatMessage(ChatRole.User, RunChatTestHelper.ShortPrompt) };
    //    var count = 0;
    //    await foreach (var _ in _kernel.RunChatStreamingAsync(messages))
    //    {
    //        count++;
    //    }

    //    Assert.IsTrue(count > 0);
    //}

    //[TestMethod]
    //public async Task RunChatStreamingAsync_SingleMessageOverload_YieldsUpdates()
    //{
    //    if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
    //    {
    //        Assert.Inconclusive("当前 AiPlatform 不支持流式输出，已跳过。");
    //        return;
    //    }

    //    var message = new ChatMessage(ChatRole.User, RunChatTestHelper.ShortPrompt);
    //    var count = 0;
    //    await foreach (var _ in _kernel.RunChatStreamingAsync(message))
    //    {
    //        count++;
    //    }

    //    Assert.IsTrue(count > 0);
    //}

    //[TestMethod]
    //public async Task RunChatAsync_WithoutChatConfig_Throws()
    //{
    //    var handler = RunChatTestHelper.CreateHandler(_senparcAiSetting);
    //    var embeddingRun = handler.IWantTo()
    //        .ConfigModel(ConfigModel.TextEmbedding, RunChatTestHelper.DefaultUserId)
    //        .BuildKernel();

    //    await Assert.ThrowsAsync<Exception>(
    //        () => embeddingRun.Kernel.RunChatAsync("test"));
    //}
}
