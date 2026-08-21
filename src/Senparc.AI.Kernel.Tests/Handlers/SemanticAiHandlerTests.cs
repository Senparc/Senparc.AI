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

            var iWantToRun = handler.ChatConfig(parameter, userId: "Jeffrey", maxHistoryStore: 10);
            //var iWantToRun = chatConfig.iWantToRun;

            //first conversation round
            var dt = SystemTime.Now;
            var prompt = "What is the town with the highest textile capacity in China in 2020?";
            var result = await handler.ChatAsync(iWantToRun, prompt);

            await Console.Out.WriteLineAsync($"first conversation round(Elapsed time:{SystemTime.DiffTotalMS(dt)}ms)");

            Assert.IsNotNull(result);
            //await Console.Out.WriteLineAsync(result.ToJson(true));
            Assert.IsTrue(result.OutputString.Length > 0);
            Assert.IsTrue(result.LastException == null);

            ((SenparcAiArguments)result.InputContext).KernelArguments.TryGetValue("human_input", out var question);
            await Console.Out.WriteLineAsync("Q: " + question);
            await Console.Out.WriteLineAsync("A: " + result.OutputString);
            await Console.Out.WriteLineAsync();

            //second conversation round
            dt = SystemTime.Now;
            prompt = "tell me more about that city. including GDP.";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"second conversation round(Elapsed time:{SystemTime.DiffTotalMS(dt)}ms)");

            ((SenparcAiArguments)result.InputContext).KernelArguments.TryGetValue("human_input", out var question2);
            await Console.Out.WriteLineAsync("Q: " + question2);
            await Console.Out.WriteLineAsync("A: " + result.OutputString);
            await Console.Out.WriteLineAsync();

            //third conversation round
            dt = SystemTime.Now;
            prompt = "what's the population of there?";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"third conversation round(Elapsed time:{SystemTime.DiffTotalMS(dt)}ms)");

            ((SenparcAiArguments)result.InputContext).KernelArguments.TryGetValue("human_input", out var question3);
            await Console.Out.WriteLineAsync("Q: " + question3);
            await Console.Out.WriteLineAsync("A: " + result.OutputString);
            await Console.Out.WriteLineAsync();

            //fourth conversation round
            dt = SystemTime.Now;
            prompt = "Translate the answer to the GDP question above into Chinese.";
            result = await handler.ChatAsync(iWantToRun, prompt);
            await Console.Out.WriteLineAsync($"fourth conversation round(Elapsed time:{SystemTime.DiffTotalMS(dt)}ms)");
            ((SenparcAiArguments)result.InputContext).KernelArguments.TryGetValue("human_input", out var question4);
            await Console.Out.WriteLineAsync("Q: " + question4);
            await Console.Out.WriteLineAsync("A: " + result.OutputString);
        }

        [TestMethod]
        public async Task ReadMeDemoTest()
        {
            //Create the AI Handler(can also be injected through a factory)
            var handler = new SemanticAiHandler(Senparc.AI.Config.SenparcAiSetting);

            //Define AI API call parameters, token limits, and related settings
            var promptParameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            //Prepare to run
            var userId = "JeffreySu";//Distinguish users
            var iWantToRun =
                 handler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId)
                        .BuildKernel()
                        .CreateFunctionFromPrompt(Senparc.AI.DefaultSetting.GetPromptForChat(), promptParameter)
                        .iWantToRun;

            // settinginput/Question
            var prompt = "What is the population of China?";
            var aiRequest = iWantToRun.CreateRequest(true)
                                      .SetStoredContext("human_input", prompt);

            //Initialize conversation history (optional)
            if (!aiRequest.GetStoredArguments("history", out var history))
            {
                aiRequest.SetStoredContext("history", "");
            }

            //Execute and return the result
            var aiResult = await iWantToRun.RunAsync(aiRequest);

            //Record conversation history (optional)
            aiRequest.SetStoredContext("history", history + $"\nHuman: {prompt}\nBot: {aiRequest.RequestContent}");

            //aiResult.Result Result:The population of China is about 1.38 billion.
            await Console.Out.WriteLineAsync(aiResult.OutputString);
            //await Console.Out.WriteLineAsync(aiResult.ToJson(true));

            //Second chat, includes context and automatically understands that the question target is population count
            aiRequest.SetStoredContext("human_input", "What about the United States?");

            aiResult = await iWantToRun.RunAsync(aiRequest);
            //aiResult.Result Result:The population of the United States is about 320 million.
            await Console.Out.WriteLineAsync(aiResult.OutputString);
        }

        [TestMethod]
        public async Task PureFunctionTextCompletionTest1()
        {
            //Create the AI Handler(can also be injected through a factory)
            var setting = Senparc.AI.Config.SenparcAiSetting;
            var handler = new SemanticAiHandler(setting);

            //Define AI API call parameters, token limits, and related settings
            var promptParameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var functionPrompt = @"Use creative language where possible to complete the following text: {{$input}}. Keep the original format and any matching writing style in mind.";

            //Prepare to run
            var userId = "JeffreySu";//Distinguish users
            var modelName = Senparc.AI.Config.SenparcAiSetting.ModelName.Chat;//default model
            var iWantToRun =
                 handler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId)
                        .BuildKernel()
                        .CreateFunctionFromPrompt(functionPrompt, promptParameter).iWantToRun;

            var request = iWantToRun.CreateRequest("Moonlight before my bed, ", true);
            var result = await iWantToRun.RunAsync(request);

            //await Console.Out.WriteLineAsync(Senparc.AI.Config.SenparcAiSetting.ToJson(true));

            Assert.IsNotNull(result);
            await Console.Out.WriteLineAsync(result.OutputString);
        }

        [TestMethod]
        public async Task PureFunctionTextCompletionTest2()
        {
            //Create the AI Handler(can also be injected through a factory)
            var handler = new SemanticAiHandler(Senparc.AI.Config.SenparcAiSetting);

            //Define AI API call parameters, token limits, and related settings
            var promptParameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var funtcionPrompt = @"Process the text according to the new text requirements:
1. Trim leading and trailing spaces or line breaks from the sentence.
2. Capitalize the first letter after punctuation.
3. Do not change the first letter of words in the middle of the sentence. Keep the original casing.
4. Ignore case conversion for proper nouns and keep the original casing.
5. Trim leading and trailing whitespace from the text.
6. Remove spaces between words.
7. Output only the result. Do not output any other text.

sample:
+++++++
#Input:
 My nam e Is Jef fre y, I'm a Chinese  . this is A test.HappY bIrthday  !
#Output:
MynameIsJeffrey,I'maChinese.ThisisAtest.HappYbIrthday!
+++++++

" +
@"#Input:{{$INPUT}}
#Output:";

            //Prepare to run
            var userId = "JeffreySu";//Distinguish users

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

            // Find all matches
            MatchCollection matches = Regex.Matches(history, pattern, RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                Console.WriteLine(match.Value);
            }

            Assert.AreEqual(5, matches.Count);

            //keep 1 itemsHistory records
            var maxHistoryCount = 1;
            var result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(@"ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Human: Same
ChatBot: Same", result);


            //keep 2 itemsHistory records
            maxHistoryCount = 2;
            result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(@"ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

Human: Same
ChatBot: Same
Human: Same
ChatBot: Same", result);

            //keep 3 itemsHistory records
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

            //keep 4 itemsHistory records
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

            //keep 5 itemsHistory records:keep all
            maxHistoryCount = 5;
            result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(history, result);

            //keep 6 itemsHistory records:keep all
            maxHistoryCount = 6;
            result = handler.RemoveHistory(history, maxHistoryCount);
            Assert.AreEqual(history, result);
        }
    }
}
