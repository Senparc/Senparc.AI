using Senparc.AI.Entities;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Kernel.Tests.Handlers
{
    [TestClass]
    public class SemanticAiHandlerTest : KernelTestBase
    {
        [TestMethod]
        public async Task ChatAsyncTest()
        {
            var handler = new SemanticAiHandler();//IAiHandler

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var chatConfig = handler.ChatConfig(parameter, userId: "Jeffrey", KernelTestBase.Default_TextCompletion);
            var iWantToRun = chatConfig.iWantToRun;

            //第一轮对话
            var dt = SystemTime.Now;
            var prompt = "What is the town with the highest textile capacity in China in 2020?";
            var result = await handler.ChatAsync(iWantToRun, prompt);

            await Console.Out.WriteLineAsync($"第一轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");

            Assert.IsNotNull(result);
            //await Console.Out.WriteLineAsync(result.ToJson(true));
            Assert.IsTrue(result.Output.Length > 0);
            Assert.IsTrue(result.LastException == null);

            ((SenparcAiContext)result.InputContext).ExtendContext.TryGetValue("human_input", out var question);
            await Console.Out.WriteLineAsync("Q: " + question);
            await Console.Out.WriteLineAsync("A: " + result.Output);
            await Console.Out.WriteLineAsync();

            //第二轮对话
            dt = SystemTime.Now;
            prompt = "tell me more about that city. including GDP.";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"第二轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");

            ((SenparcAiContext)result.InputContext).ExtendContext.TryGetValue("human_input", out var question2);
            await Console.Out.WriteLineAsync("Q: " + question2);
            await Console.Out.WriteLineAsync("A: " + result.Output);
            await Console.Out.WriteLineAsync();

            //第三轮对话
            dt = SystemTime.Now;
            prompt = "what's the population of there?";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"第三轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");

            ((SenparcAiContext)result.InputContext).ExtendContext.TryGetValue("human_input", out var question3);
            await Console.Out.WriteLineAsync("Q: " + question3);
            await Console.Out.WriteLineAsync("A: " + result.Output);
            await Console.Out.WriteLineAsync();

            //第四轮对话
            dt = SystemTime.Now;
            prompt = "将上面包含GDP那一条提问的回答，翻译成中文。";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"第四轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");
            ((SenparcAiContext)result.InputContext).ExtendContext.TryGetValue("human_input", out var question4);
            await Console.Out.WriteLineAsync("Q: " + question4);
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
            var modelName = KernelTestBase.Default_TextCompletion;//默认使用模型
            var iWantToRun =
                 handler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId, modelName)
                        .BuildKernel()
                        .RegisterSemanticFunction("ChatBot", "Chat", promptParameter)
                        .iWantToRun;

            // 设置输入/提问
            var prompt = "请问中国有多少人口？";
            var aiRequest = iWantToRun.CreateRequest(true)
                                      .SetStoredContext("human_input", prompt);

            //初始化对话历史（可选）
            if (!aiRequest.GetStoredContext("history", out var history))
            {
                aiRequest.SetStoredContext("history", "");
            }

            //执行并返回结果
            var aiResult = await iWantToRun.RunAsync(aiRequest);

            //记录对话历史（可选）
            aiRequest.SetStoredContext("history", history + $"\nHuman: {prompt}\nBot: {aiRequest.RequestContent}");

            //aiResult.Result 结果：中国的人口约为13.8亿。
            await Console.Out.WriteLineAsync(aiResult.Output);
            //await Console.Out.WriteLineAsync(aiResult.ToJson(true));

            //第二次对话，包含上下文，自动理解提问目标是人口数量
            aiRequest.SetStoredContext("human_input", "那美国呢");

            aiResult = await iWantToRun.RunAsync(aiRequest);
            //aiResult.Result 结果：美国的人口大约为3.2亿。
            await Console.Out.WriteLineAsync(aiResult.Output);
        }

        [TestMethod]
        public async Task TextCompletionTest()
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
            var modelName = KernelTestBase.Default_TextCompletion;//默认使用模型
            var iWantToRun =
                 handler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId, modelName)
                        .BuildKernel();

            var prompt = "床前明月光，疑似_";
            var request = iWantToRun.CreateRequest(prompt);
            var result = await iWantToRun.RunAsync(request);

            await Console.Out.WriteLineAsync(Senparc.AI.Config.SenparcAiSetting.ToJson(true));

            Assert.IsNotNull(result);
            await Console.Out.WriteLineAsync(result.Output);
        }
    }
}
