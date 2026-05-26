using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.IWantToExtensions;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System.Text;

namespace Senparc.AI.AgentKernel.Tests.IWantToExtensions;

[TestClass]
public class IWantToRunExtensionRunChatTests : KernelTestBase
{
    [TestMethod]
    public async Task RunChat_ReturnsAgentResponse()
    {
        var iWantToRun = await  RunChatTestHelper.BuildChatRun();
        var response = await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt);

        RunChatTestHelper.AssertAgentResponse(response.Result);
        Console.WriteLine($"[RunChat] {response.Result.Text}");
    }

    [TestMethod]
    public async Task RunChat_Generic_ReturnsTypedResponse()
    {
        var iWantToRun = await RunChatTestHelper.BuildChatRun();
        var response = await iWantToRun.RunChatAsync<string>(RunChatTestHelper.ShortPrompt);

        RunChatTestHelper.AssertAgentResponse(response.Result);
        Console.WriteLine($"[RunChat] {response.Result.Text}");
    }

    [TestMethod]
    public async Task RunChat_WithAgentSession_UsesSession()
    {
        var iWantToRun = await RunChatTestHelper.BuildChatRunWithSessionAsync();
        var session = iWantToRun.Kernel.AgentSession;

        var response = await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt, session);

        RunChatTestHelper.AssertAgentResponse(response.Result);
    }

    [TestMethod]
    public async Task RunChatStreaming_StringPrompt_YieldsUpdates()
    {
        if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
        {
            Assert.Inconclusive("当前 AiPlatform 不支持流式输出，已跳过。");
            return;
        }

        var iWantToRun = await RunChatTestHelper.BuildChatRun();
        StringBuilder sb = new();

        await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt, null, update => {
            sb.AppendLine($"{sb.Length.ToString("000")} {update.Role} {update.Text}  / {update.FinishReason} {update.RawRepresentation}");
            Assert.IsNotNull(update);
        });

        Assert.IsTrue(sb.Length > 0);
        Console.WriteLine(sb.ToString());
    }

    [TestMethod]
    public async Task RunChatStreaming_ChatMessages_YieldsUpdates()
    {
        if (!RunChatTestHelper.SupportsStreaming(_senparcAiSetting))
        {
            Assert.Inconclusive("当前 AiPlatform 不支持流式输出，已跳过。");
            return;
        }

        var iWantToRun = await RunChatTestHelper.BuildChatRun();
        var messages = new[] { new ChatMessage(ChatRole.User, RunChatTestHelper.ShortPrompt) };
        StringBuilder sb = new();

        List<AgentResponseUpdate> list = new();
        await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt, null, update => {
            sb.AppendLine($"{sb.Length.ToString("000")} {update.Role} {update.Text}  / {update.FinishReason} {update.RawRepresentation}");
            Assert.IsNotNull(update);
            list.Add(update);
        });

        var s = list.Where(z => z.FinishReason == ChatFinishReason.Stop);

        Assert.IsTrue(sb.Length > 0);
        Console.WriteLine(sb.ToString());
    }
}
