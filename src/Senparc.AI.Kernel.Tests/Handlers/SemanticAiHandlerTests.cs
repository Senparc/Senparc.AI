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

            var iWantToRun = await handler.ChatConfigAsync(parameter, userId: "Jeffrey");

            //第一轮对话
            var dt = SystemTime.Now;
            var prompt = "What is the town with the highest textile capacity in China in 2020?";
            var request = new SenparcAiRequest("Jeffrey", "text-davinci-003", prompt, parameter);
            var result = await handler.ChatAsync(iWantToRun, request);

            await Console.Out.WriteLineAsync($"第一轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");

            Assert.IsNotNull(result);
            await Console.Out.WriteLineAsync(result.ToJson(true));
            Assert.IsNotNull(result.Output);
            Assert.IsTrue(result.Output.Length > 0);
            Assert.IsTrue(result.LastException == null);

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
    }
}
