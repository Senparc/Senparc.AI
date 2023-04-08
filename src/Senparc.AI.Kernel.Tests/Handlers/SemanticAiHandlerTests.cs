using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Entities;
using Senparc.AI.Kernel.Helpers;
using Senparc.AI.Kernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.AI.Kernel.Handlers;

namespace Senparc.AI.Kernel.Tests.Handlers
{
    [TestClass]
    public class SemanticAiHandlerTest : KernelTestBase
    {
        [TestMethod]
        public async Task ChatAsyncTest()
        {
            var helper = new SemanticKernelHelper();
            var handler = new SemanticAiHandler(helper);

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var chatConfig = handler.ChatConfig(parameter, userId: "Jeffrey");
            var iWantToRun = chatConfig.iWantToRun;
            var chatFunction = chatConfig.chatFunction;

            //第一轮对话
            var dt = SystemTime.Now;
            var prompt = "What is the town with the highest textile capacity in China in 2020?";
            var request = new SenparcAiRequest("Jeffrey", "text-davinci-003", prompt, parameter, true, chatFunction);
            var result = await handler.ChatAsync(iWantToRun, request);

            await Console.Out.WriteLineAsync($"第一轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");

            Assert.IsNotNull(result);
            //await Console.Out.WriteLineAsync(result.ToJson(true));
            Assert.IsTrue(result.Output.Length > 0);
            Assert.IsTrue(result.LastException == null);

            await Console.Out.WriteLineAsync("Q: " + result.Input);
            await Console.Out.WriteLineAsync("A: " + result.Output);
            await Console.Out.WriteLineAsync();

            //第二轮对话
            dt = SystemTime.Now;
            request.RequestContent = "tell me more about that city. including GDP.";
            result = await handler.ChatAsync(iWantToRun, request);
            await Console.Out.WriteLineAsync($"第二轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");
            await Console.Out.WriteLineAsync("Q: " + result.Input);
            await Console.Out.WriteLineAsync("A: " + result.Output);
            await Console.Out.WriteLineAsync();

            //第三轮对话
            dt = SystemTime.Now;
            request.RequestContent = "what's the population of there?";
            result = await handler.ChatAsync(iWantToRun, request);
            await Console.Out.WriteLineAsync($"第三轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");
            await Console.Out.WriteLineAsync("Q: " + result.Input);
            await Console.Out.WriteLineAsync("A: " + result.Output);
            await Console.Out.WriteLineAsync();

            //第四轮对话
            dt = SystemTime.Now;
            request.RequestContent = "将上面包含GDP那一条提问的回答，翻译成中文。";
            result = await handler.ChatAsync(iWantToRun, request);
            await Console.Out.WriteLineAsync($"第四轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");
            await Console.Out.WriteLineAsync("Q: " + result.Input);
            await Console.Out.WriteLineAsync("A: " + result.Output);
        }

        [TestMethod]
        public async Task ReadMeDemoTest()
        {
            //创建 AI Handler 处理器（也可以通过工厂依赖注入）
            var handler = new SemanticAiHandler();

            //定义 AI 接口调用参数和 Token 限制等
            var promptParameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            //准备运行
            var userId = "JeffreySu";//区分用户
            var modelName = "text-davinci-003";//默认使用模型
            var iWantToRun =
                 handler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId, modelName)
                        .BuildKernel()
                        .RegisterSemanticFunction("ChatBot", "Chat", promptParameter)
                        .iWantToRun;

            // 输入/提问，获取结果
            var prompt = "请问中国有多少人口？";
            var aiRequest = iWantToRun.CreateRequest(prompt, true, true);
            var aiResult = await iWantToRun.RunAsync(aiRequest);
            //aiResult.Result 结果：中国的人口约为13.8亿。
            await Console.Out.WriteLineAsync(aiResult.Output);
            //await Console.Out.WriteLineAsync(aiResult.ToJson(true));

            //第二次对话，包含上下文，自动理解提问目标是人口数量
            aiRequest.RequestContent = "那么美国呢？";
            aiResult = await iWantToRun.RunAsync(aiRequest);
            //aiResult.Result 结果：美国的人口大约为3.2亿。
            await Console.Out.WriteLineAsync(aiResult.Output);

        }
    }
}
