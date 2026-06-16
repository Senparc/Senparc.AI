using Azure.Storage.Blobs.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.IWantToExtensions;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.Tests.IWantToExtensions;

[TestClass]
public class IWantToRunExtensionRunChatTests : KernelTestBase
{
    [TestMethod]
    public async Task RunChat_ReturnsAgentResponse()
    {
        var iWantToRun = await RunChatTestHelper.BuildChatRun();
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

        await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt, null, update =>
        {
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
        await iWantToRun.RunChatAsync(RunChatTestHelper.ShortPrompt, null, update =>
        {
            sb.AppendLine($"{sb.Length.ToString("000")} {update.Role} {update.Text}  / {update.FinishReason} {update.RawRepresentation}");
            Assert.IsNotNull(update);
            list.Add(update);
        });

        var s = list.Where(z => z.FinishReason == ChatFinishReason.Stop);

        Assert.IsTrue(sb.Length > 0);
        Console.WriteLine(sb.ToString());
    }


    [TestMethod]
    public async Task EntityClassToolsTest()
    {
        var functionCall = new FunctionCall
        {
            Name = "TestFunction",
            Arguments = Guid.NewGuid().ToString("n")
        };

        var aiHandler = new AgentAiHandler(BaseSupport.KernelTestBase._senparcAiSetting);
        var iWantToRun = aiHandler.IWantTo().ConfigChatModel("SenparcAiTest", new ChatClientAgentOptions()
        {
            ChatOptions = new ChatOptions()
            {
                Instructions = "你是一个会调用工具的机器人，负责使用工具（function-calling）处理问题，只要能够用工具，就不要用推理回答问题。",
                MaxOutputTokens = 2000,
                TopP = 0.2f,
                Temperature = 0.2f,
                Tools = [
                   AIFunctionFactory.Create(functionCall.Now),
                   AIFunctionFactory.Create(functionCall.Echo),
                   AIFunctionFactory.Create(functionCall.CalcPlus),
                    ],
                AllowMultipleToolCalls = true
            }
        }).BuildKernel();

        //Single Tool
        {
            var result = await iWantToRun.RunChatAsync("请问现在几点钟？");
            Console.WriteLine(result.OutputString);
            Assert.IsTrue(result.OutputString.Contains(SystemTime.Now.Year.ToString()));
        }

        //Multiple Tools
        {
            var result = await iWantToRun.RunChatAsync("请问现在几点钟？把获取到的时间中，提取当前的年份数字，加上100形成一个新的数字，然后当作字符串进行处理，最后告诉我这个字符串结果。");
            Console.WriteLine(result.OutputString);
            Assert.IsTrue(result.OutputString.Contains((SystemTime.Now.Year + 100).ToString()));
            Assert.IsTrue(result.OutputString.Contains(functionCall.Name));
            Assert.IsTrue(result.OutputString.Contains(functionCall.Arguments));

            Console.WriteLine("用量：" + result.Result.Usage.ToJson(true));
        }


    }

}

public class FunctionCall
{
    public string Name { get; set; }
    public string Arguments { get; set; }



    [Description("获取当前时间")]
    public DateTime Now()
    {
        return DateTime.Now;
    }

    [Description("计算两个数的和")]
    public double CalcPlus(double x, double y)
    {
        return x + y;
    }

    [Description("处理字符串")]
    public string Echo(string input)
    {
        return $"Echo: {input} + {Name} +{Arguments}";
    }
}