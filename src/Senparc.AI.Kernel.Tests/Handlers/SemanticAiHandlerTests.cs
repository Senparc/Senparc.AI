using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel;
using Microsoft.SemanticKernel;
using Senparc.AI.Entities;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.KernelConfigExtensions;
using Senparc.AI.Kernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System.Formats.Asn1;
using System.Text.RegularExpressions;

namespace Senparc.AI.Kernel.Tests.Handlers
{
    [TestClass]
    public class SemanticAiHandlerTest : KernelTestBase
    {
        [TestMethod]
        public async Task ChatAsyncTest()
        {
            var handler = new SemanticAiHandler(Senparc.AI.Config.SenparcAiSetting);//IAiHandler

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 1000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var chatConfig = handler.ChatConfig(parameter, userId: "Jeffrey", maxHistoryStore: 10);
            var iWantToRun = chatConfig.iWantToRun;

            //第一轮对话
            var dt = SystemTime.Now;
            var prompt = "What is the town with the highest textile capacity in China in 2020?";
            var result = await handler.ChatAsync(iWantToRun, prompt);

            await Console.Out.WriteLineAsync($"第一轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");

            Assert.IsNotNull(result);
            //await Console.Out.WriteLineAsync(result.ToJson(true));
            Assert.IsTrue(result.OutputString.Length > 0);
            Assert.IsTrue(result.LastException == null);

            ((SenparcAiArguments)result.InputContext).KernelArguments.TryGetValue("human_input", out var question);
            await Console.Out.WriteLineAsync("Q: " + question);
            await Console.Out.WriteLineAsync("A: " + result.OutputString);
            await Console.Out.WriteLineAsync();

            //第二轮对话
            dt = SystemTime.Now;
            prompt = "tell me more about that city. including GDP.";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"第二轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");

            ((SenparcAiArguments)result.InputContext).KernelArguments.TryGetValue("human_input", out var question2);
            await Console.Out.WriteLineAsync("Q: " + question2);
            await Console.Out.WriteLineAsync("A: " + result.OutputString);
            await Console.Out.WriteLineAsync();

            //第三轮对话
            dt = SystemTime.Now;
            prompt = "what's the population of there?";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"第三轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");

            ((SenparcAiArguments)result.InputContext).KernelArguments.TryGetValue("human_input", out var question3);
            await Console.Out.WriteLineAsync("Q: " + question3);
            await Console.Out.WriteLineAsync("A: " + result.OutputString);
            await Console.Out.WriteLineAsync();

            //第四轮对话
            dt = SystemTime.Now;
            prompt = "将上面包含GDP那一条提问的回答，翻译成中文。";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"第四轮对话（耗时：{SystemTime.DiffTotalMS(dt)}ms）");
            ((SenparcAiArguments)result.InputContext).KernelArguments.TryGetValue("human_input", out var question4);
            await Console.Out.WriteLineAsync("Q: " + question4);
            await Console.Out.WriteLineAsync("A: " + result.OutputString);
        }

        [TestMethod]
        public async Task ReadMeDemoTest()
        {
            //创建 AI Handler 处理器（也可以通过工厂依赖注入）
            var handler = new SemanticAiHandler(Senparc.AI.Config.SenparcAiSetting);

            //定义 AI 接口调用参数和 Token 限制等
            var promptParameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            //准备运行
            var userId = "JeffreySu";//区分用户
            var iWantToRun =
                 handler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId)
                        .BuildKernel()
                        .CreateFunctionFromPrompt(Senparc.AI.DefaultSetting.GetPromptForChat(), promptParameter)
                        .iWantToRun;

            // 设置输入/提问
            var prompt = "请问中国有多少人口？";
            var aiRequest = iWantToRun.CreateRequest(true)
                                      .SetStoredContext("human_input", prompt);

            //初始化对话历史（可选）
            if (!aiRequest.GetStoredArguments("history", out var history))
            {
                aiRequest.SetStoredContext("history", "");
            }

            //执行并返回结果
            var aiResult = await iWantToRun.RunAsync(aiRequest);

            //记录对话历史（可选）
            aiRequest.SetStoredContext("history", history + $"\nHuman: {prompt}\nBot: {aiRequest.RequestContent}");

            //aiResult.Result 结果：中国的人口约为13.8亿。
            await Console.Out.WriteLineAsync(aiResult.OutputString);
            //await Console.Out.WriteLineAsync(aiResult.ToJson(true));

            //第二次对话，包含上下文，自动理解提问目标是人口数量
            aiRequest.SetStoredContext("human_input", "那美国呢");

            aiResult = await iWantToRun.RunAsync(aiRequest);
            //aiResult.Result 结果：美国的人口大约为3.2亿。
            await Console.Out.WriteLineAsync(aiResult.OutputString);
        }

        [TestMethod]
        public async Task PureFunctionTextCompletionTest1()
        {
            //创建 AI Handler 处理器（也可以通过工厂依赖注入）
            var setting = Senparc.AI.Config.SenparcAiSetting;
            var handler = new SemanticAiHandler(setting);

            //定义 AI 接口调用参数和 Token 限制等
            var promptParameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var functionPrompt = @"请使用尽量有创造性的语言，补全下面的文字：{{$input}}，请注意原文的格式，和可能匹配的文体。";

            //准备运行
            var userId = "JeffreySu";//区分用户
            var modelName = Senparc.AI.Config.SenparcAiSetting.ModelName.Chat;//默认使用模型
            var iWantToRun =
                 handler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId)
                        .BuildKernel()
                        .CreateFunctionFromPrompt(functionPrompt, promptParameter).iWantToRun;

            var request = iWantToRun.CreateRequest("床前明月光，", true);
            var result = await iWantToRun.RunAsync(request);

            //await Console.Out.WriteLineAsync(Senparc.AI.Config.SenparcAiSetting.ToJson(true));

            Assert.IsNotNull(result);
            await Console.Out.WriteLineAsync(result.OutputString);
        }

        [TestMethod]
        public async Task PureFunctionTextCompletionTest2()
        {
            //创建 AI Handler 处理器（也可以通过工厂依赖注入）
            var handler = new SemanticAiHandler(Senparc.AI.Config.SenparcAiSetting);

            //定义 AI 接口调用参数和 Token 限制等
            var promptParameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var funtcionPrompt = @"请根据新文本要求处理文字：
1. 将标点符号后的首字母改成大写；在句子中间的单词，首字母不需要改写，请保留原来的大小写。
2. 忽略专有名词的大小写转换，请保留原文的大小写。
3. 去掉文字首尾的空格。
4. 去掉文字之间的空格。
5. 直接输出结果，不需要输出任何其他文字。

示例：
+++++++
#Input:
My nam e Is Jef fre y, I'm a Chinese  . this is A test.HappY bIrthday  !  
#Output:
MynameIsJeffrey,I'maChinese.ThisisAtest.HappYbIrthday!
+++++++

" +
@"#Input:{{$INPUT}}
#Output:";

            //准备运行
            var userId = "JeffreySu";//区分用户

            var iWantToRun =
                 handler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId)
                        .BuildKernel();

            iWantToRun.CreateFunctionFromPrompt(funtcionPrompt, promptParameter);

            var request = iWantToRun.CreateRequest("  he llo w orld !  thi s is a n ew w orld.  ", true);
            var result = await iWantToRun.RunAsync(request);

            //await Console.Out.WriteLineAsync(Senparc.AI.Config.SenparcAiSetting.ToJson(true));

            Assert.IsNotNull(result);
            await Console.Out.WriteLineAsync(result.OutputString);
            Assert.AreEqual("Helloworld!Thisisanewworld.", result.OutputString);
        }

        [TestMethod()]
        public void RemoveHistoryTest()
        {
            var handler = new SemanticAiHandler(Senparc.AI.Config.SenparcAiSetting);

            var history = @"ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Human: One
Still One
ChatBot: Two
Still Tow
Human: Three
ChatBot:Four
Human: Five
Still File
ChatBot: Six
Still Six
Human: Same
ChatBot: Same
Human: Same
ChatBot: Same";

            string pattern = @"Human:.*?ChatBot:.*?(?=(Human:|$))";

            // 找到所有的匹配  
            MatchCollection matches = Regex.Matches(history, pattern, RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                Console.WriteLine(match.Value);
            }

            Assert.AreEqual(5, matches.Count);

            //保留 1 条历史记录
            var maxHistoryCount = 1;
            var result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(@"ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Human: Same
ChatBot: Same", result);


            //保留 2 条历史记录
            maxHistoryCount = 2;
            result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(@"ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Human: Same
ChatBot: Same
Human: Same
ChatBot: Same", result);

            //保留 3 条历史记录
            maxHistoryCount = 3;
            result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(@"ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Human: Five
Still File
ChatBot: Six
Still Six
Human: Same
ChatBot: Same
Human: Same
ChatBot: Same", result);

            //保留 4 条历史记录
            maxHistoryCount = 4;
            result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(@"ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Human: Three
ChatBot:Four
Human: Five
Still File
ChatBot: Six
Still Six
Human: Same
ChatBot: Same
Human: Same
ChatBot: Same", result);

            //保留 5 条历史记录：全部保留
            maxHistoryCount = 5;
            result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(history, result);

            //保留 6 条历史记录：全部保留
            maxHistoryCount = 6;
            result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(history, result);
        }
    }
}
