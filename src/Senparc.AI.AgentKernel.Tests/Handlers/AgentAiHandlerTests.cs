using Microsoft.Agents.AI;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Helpers;
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
            var agentAiHandler = new AgentAiHandler();
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

            var result = await iWantToRun.RunChatAsync("Hello, how are you ? ");
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
                         .RunChatAsync("How is cosine similarity used in AI training and RAG? Wrap every 50 words. ");

            Assert.IsTrue(result.Result.Text.Length > 0);
            Console.WriteLine(result.Result.Text);
        }

        [TestMethod]
        public async Task ConversationTestWithSession()
        {
            AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

            var prompts = new[] { "What are the local specialties of Suzhou(please list three)?", "What about Nanjing?", "What about Beijing?in English" };
            var i = 0;
            AgentSession agentSession = null;
            foreach (var prompt in prompts)
            {
                var iWantToRun = await agentAiHandler.IWantTo()
                           .ConfigModel(ConfigModel.Chat, "Jeffrey")
                           .BuildKernelWithAgentSessionAsync();

                agentSession ??= iWantToRun.Kernel.AgentSession;
                Console.WriteLine("AgentSession is null:" + (agentSession == null));

                var result = await iWantToRun.RunChatAsync(prompt, agentSession);

                Console.WriteLine($"[{i}]Result:{result.Result.Text}(Tokens-input {result.Result.Usage.InputTokenCount} output {result.Result.Usage.OutputTokenCount})");

                if (i == 1)
                {
                    Assert.Contains("salted duck", result.Result.Text, "The response should contain 'salted duck' for the first prompt.");
                }

                i++;
            }
        }


        [TestMethod]
        public async Task ConversationTestWithDefaultSession()
        {
            AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

            var prompts = new[] { "What are the local specialties of Suzhou(please list three)?", "What about Nanjing?", "What about Beijing?in English" };
            var i = 0;
            AgentSession agentSession = null;

            var iWantToRun = agentAiHandler.IWantTo()
                      .ConfigModel(ConfigModel.Chat, "Jeffrey")
                      .BuildKernel(); //The same object includes its own Session

            foreach (var prompt in prompts)
            {
                agentSession ??= iWantToRun.Kernel.AgentSession;
                Console.WriteLine("AgentSession is null:" + (agentSession == null));

                var result = await iWantToRun.RunChatAsync(prompt, agentSession);

                Console.WriteLine($"[{i}]Result:{result.Result.Text}(Tokens-input {result.Result.Usage.InputTokenCount} output {result.Result.Usage.OutputTokenCount})");

                if (i == 1)
                {
                    Assert.DoesNotContain("salted duck", result.Result.Text, "The response should not contain 'salted duck' for the first prompt.");
                }

                i++;
            }
        }


        [TestMethod]
        public async Task ConversationTestWithoutSession()
        {
            AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

            var prompts = new[] { "What are the local specialties of Suzhou(please list three)?", "What about Nanjing?", "What about Beijing?in English" };
            var i = 0;
            AgentSession agentSession = null;

            foreach (var prompt in prompts)
            {
                var iWantToRun = agentAiHandler.IWantTo()
                           .ConfigModel(ConfigModel.Chat, "Jeffrey")
                           .BuildKernel();

                agentSession ??= iWantToRun.Kernel.AgentSession;
                Console.WriteLine("AgentSession is null:" + (agentSession == null));

                var result = await iWantToRun.RunChatAsync(prompt, agentSession);

                Console.WriteLine($"[{i}]Result:{result.Result.Text}(Tokens-input {result.Result.Usage.InputTokenCount} output {result.Result.Usage.OutputTokenCount})");

                if (i == 1)
                {
                    Assert.DoesNotContain("salted duck", result.Result.Text, "The response should not contain 'salted duck' for the first prompt.");
                }

                i++;
            }
        }
    }
}
