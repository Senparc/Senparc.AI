using Azure.Core;
using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System.Text;

namespace Senparc.AI.AgentKernel.Tests.KernelConfigExtensions;

[TestClass]
public class KernelConfigExtensionsRunChatTests : KernelTestBase
{
    [TestMethod]
    public async Task RunAsync_StringPrompt_ReturnsSenparcKernelAiResult()
    {
        var iWantToRun = await RunChatTestHelper.BuildChatRun();
        var result = await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt);

        Assert.IsNotNull(result);
        Assert.AreEqual(RunChatTestHelper.ShortPrompt, result.InputContent);
        RunChatTestHelper.AssertAgentResponse(result.Result);
        Assert.AreEqual(result.Result.Text, result.OutputString);
        Console.WriteLine($"[RunAsync string] {result.OutputString}");
    }

    [TestMethod]
    public async Task RunAsync_SenparcAiRequest_NonStream_ReturnsResult()
    {
        var iWantToRun = await RunChatTestHelper.BuildChatRun();
        var request = iWantToRun.CreateRequest(RunChatTestHelper.ShortPrompt, null, true);
        var result = await iWantToRun.RunChatAsync(request);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.OutputString));
        RunChatTestHelper.AssertAgentResponse(result.Result);
    }

    [TestMethod]
    public async Task RunAsync_SenparcAiRequest_GenericString_ReturnsResult()
    {
        var iWantToRun = await RunChatTestHelper.BuildChatRun();
        var request = iWantToRun.CreateRequest(RunChatTestHelper.ShortPrompt, null, true);
        var result = await iWantToRun.RunChatAsync<string>(request);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.OutputString));
        RunChatTestHelper.AssertAgentResponse(result.Result);
    }

    [TestMethod]
    public async Task RunAsync_SenparcAiRequest_WithStreamCallback_InvokesCallback()
    {
        if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
        {
            Assert.Inconclusive("current AiPlatform does not support streaming output, skipped.");
            return;
        }

        var iWantToRun = await RunChatTestHelper.BuildChatRun();
        var request = iWantToRun.CreateRequest(RunChatTestHelper.ShortPrompt, null, true);
        var callbackCount = 0;

        var result = await iWantToRun.RunChatAsync(request, _ => callbackCount++);

        Assert.IsTrue(callbackCount > 0, "The streaming callback should execute at least once");
        Assert.IsNotNull(result.StreamResult);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.OutputString));
        RunChatTestHelper.AssertAgentResponse(result.Result);
        Console.WriteLine($"[RunAsync stream] callbacks={callbackCount}, output={result.OutputString}");
    }

    [TestMethod]
    public async Task RunStreamAsync_EnablesStreamByDefault()
    {
        if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
        {
            Assert.Inconclusive("current AiPlatform does not support streaming output, skipped.");
            return;
        }

        var iWantToRun = await RunChatTestHelper.BuildChatRun();
        var request = iWantToRun.CreateRequest(RunChatTestHelper.ShortPrompt, null, true);

        StringBuilder sb = new StringBuilder();

        var result = await iWantToRun.RunChatAsync(request, item =>
        {
            sb.AppendLine(item.Text);
        });

        Assert.IsTrue(sb.Length > 0);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.OutputString));
        Console.WriteLine(sb.ToString());
    }

    [TestMethod]
    public async Task RunChatStreamingAsync_StringPrompt_ReturnsSenparcKernelAiResult()
    {
        if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
        {
            Assert.Inconclusive("current AiPlatform does not support streaming output, skipped.");
            return;
        }

        var iWantToRun = await RunChatTestHelper.BuildChatRun();

        StringBuilder sb = new StringBuilder();

        var result = await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt, null, item =>
        {
            sb.AppendLine(item.Text);
        });

        Assert.IsTrue(sb.Length > 0);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.OutputString));
        Console.WriteLine(sb.ToString());

        Assert.AreEqual(RunChatTestHelper.ShortPrompt, result.InputContent);
        Assert.IsNotNull(result.StreamResult);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.OutputString));
        RunChatTestHelper.AssertAgentResponse(result.Result);
    }

    [TestMethod]
    public async Task RunChatStreamingAsync_WithAgentSession_ReturnsResult()
    {
        if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
        {
            Assert.Inconclusive("current AiPlatform does not support streaming output, skipped.");
            return;
        }

        var iWantToRun = await RunChatTestHelper.BuildChatRunWithSessionAsync();
        var session = iWantToRun.Kernel.AgentSession;
        int count = 0;
        var result = await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt, null, update =>
        {
            count++;
            Assert.IsNotNull(update);
        });
        Assert.IsTrue(count > 0);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.OutputString));
    }
}
