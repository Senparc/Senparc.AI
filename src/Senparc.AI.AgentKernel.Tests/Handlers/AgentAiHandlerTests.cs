using Microsoft.Agents.AI;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.AgentKernel.KernelConfigExtensions;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Tests.Handlers
{
    [TestClass]
    public class AgentAiHandlerTests : KernelTestBase
    {
        [TestMethod]
        public void CreateTest()
        {
            var setting = Senparc.AI.Config.SenparcAiSetting;
            var agentAiHandler = new AgentAiHandler(setting);
            Assert.AreSame(setting, agentAiHandler.AgentKernelHelper.AiSetting);
        }

        [TestMethod]
        public async Task RunTest()
        {

            AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

            var iWantToRun =
            agentAiHandler.IWantTo()
                        .ConfigModel(ConfigModel.Chat, "Jeffrey")
                        .BuildKernel();

            var result = await iWantToRun.RunAsync("Hello, how are you ? ");
            Assert.IsTrue(result.Result.CreatedAt.Value.UtcDateTime > DateTime.UtcNow.AddMinutes(-5), "Result should be created within the last 5 minutes.");
            Assert.IsTrue(result.Result.Usage.TotalTokenCount > 0);
            Console.WriteLine($"Result: {result.Result.Text}");

            Console.WriteLine("Usage:" + result.Result.Usage.ToJson(true));
        }

        [TestMethod]
        public async Task SingleLineTest()
        {
            var result = await new AgentAiHandler()
                         .IWantTo()
                         .ConfigModel(ConfigModel.Chat, "Jeffrey")
                         .BuildKernel()
                         .RunAsync("余弦相似度在AI训练和RAG过程中，如何被使用到了？每50个字换一行。 ");

            Assert.IsTrue(result.Result.Text.Length > 0);
            Console.WriteLine(result.Result.Text);
        }

        [TestMethod]
        public async Task ConversationTestWithSession()
        {
            AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

            var prompts = new[] { "苏州特产有哪些（请您说三个）？", "南京的呢？", "北京的呢？用英文" };
            var i = 0;
            AgentSession agentSession = null;
            foreach (var prompt in prompts)
            {
                var iWantToRun =await agentAiHandler.IWantTo()
                           .ConfigModel(ConfigModel.Chat, "Jeffrey")
                           .BuildKernelWithAgentSessionAsync();

                agentSession ??= iWantToRun.Kernel.AgentSession;
                Console.WriteLine("AgentSession is null:" + (agentSession == null));

                var result = await iWantToRun.RunAsync(prompt, agentSession);

                Console.WriteLine($"[{i}]结果：{result.Result.Text}（Tokens-input {result.Result.Usage.InputTokenCount} output {result.Result.Usage.OutputTokenCount}）");

                i++;
            }
        }

        [TestMethod]
        public async Task ConversationTestWithoutSession()
        {
            AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

            var prompts = new[] { "苏州特产有哪些（请您说三个）？", "南京的呢？", "北京的呢？用英文" };
            var i = 0;
            AgentSession agentSession = null;
            foreach (var prompt in prompts)
            {
                var iWantToRun = agentAiHandler.IWantTo()
                           .ConfigModel(ConfigModel.Chat, "Jeffrey")
                           .BuildKernel();

                agentSession ??= iWantToRun.Kernel.AgentSession;
                Console.WriteLine("AgentSession is null:" + (agentSession == null));

                var result = await iWantToRun.RunAsync(prompt, agentSession);

                Console.WriteLine($"[{i}]结果：{result.Result.Text}（Tokens-input {result.Result.Usage.InputTokenCount} output {result.Result.Usage.OutputTokenCount}）");

                i++;
            }
        }
    }
}
