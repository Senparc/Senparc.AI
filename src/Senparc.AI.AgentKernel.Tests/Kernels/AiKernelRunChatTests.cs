using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    [TestMethod]
    public async Task TestPlaceHolder()
    {
        var prompt = "What local specialties does {{city}} have?";
        var iWantToRun = await new AgentAiHandler().IWantTo().ConfigChatModel("Jeffrey",
            new ChatClientAgentOptions()
            {
                ChatOptions = new ChatOptions()
                {
                    Instructions = "You are an assistant responsible for helping me answer questions. Before answering, repeat my full question."
                }
            }
            ).BuildKernelWithAgentSessionAsync();

        iWantToRun.Kernel.AgentSession.StateBag.SetValue("city", "Suzhou");

        var result = await iWantToRun.RunChatAsync(prompt, iWantToRun.Kernel.AgentSession);

        Console.WriteLine("result:" + result.OutputString);

        Assert.IsTrue(result.OutputString.Contains("Suzhou"));

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

    //    var first = await iWantToRun.Kernel.RunChatAsync("What are three local specialties of Suzhou? Only list three.", session);
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
    //        Assert.Inconclusive("current AiPlatform does not support streaming output, skipped.");
    //        return;
    //    }

    //    var updates = new List<AgentResponseUpdate>();
    //    await foreach (var update in _kernel.RunChatStreamingAsync(RunChatTestHelper.ShortPrompt))
    //    {
    //        updates.Add(update);
    //    }

    //    Assert.IsTrue(updates.Count > 0, "Streaming should return at least one AgentResponseUpdate");
    //    var text = updates.ToAgentResponse().Text ?? string.Empty;
    //    Assert.IsFalse(string.IsNullOrWhiteSpace(text), "The aggregated streaming text should not be empty");
    //    Console.WriteLine($"[RunChatStreamingAsync] chunks={updates.Count}, text={text}");
    //}

    //[TestMethod]
    //public async Task RunChatStreamingAsync_ChatMessages_YieldsUpdates()
    //{
    //    if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
    //    {
    //        Assert.Inconclusive("current AiPlatform does not support streaming output, skipped.");
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
    //        Assert.Inconclusive("current AiPlatform does not support streaming output, skipped.");
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
