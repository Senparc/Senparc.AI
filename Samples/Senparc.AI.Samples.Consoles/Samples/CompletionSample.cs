using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public class CompletionSample
    {
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public CompletionSample(IAiHandler aiHandler)
        {
            _aiHandler = aiHandler;
        }

        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync(@"CompletionSample 开始运行，请输入对话内容（不具备历史上下文）。

输入 exit 退出。");

            var promptParameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };
            ;

            var functionPrompt = @"{{$input}}";

            //准备运行
            var userId = "JeffreySu";//区分用户
            var iWantToRun =
                 _semanticAiHandler.IWantTo()
                        .ConfigModel(ConfigModel.TextCompletion, userId)
                        .BuildKernel()
                        .CreateFunctionFromPrompt(functionPrompt, promptParameter).iWantToRun;

            var multiLineContent = new StringBuilder();
            var useMultiLine = false;
            //开始对话
            while (true)
            {

                await Console.Out.WriteLineAsync("提示词：");
                var prompt = Console.ReadLine();

                if (prompt == "exit")
                {
                    break;
                }
                else if (prompt.ToUpper() == "[ML]")
                {
                    await Console.Out.WriteLineAsync("识别到多行模式，请继续输入");
                    useMultiLine = true;
                }

                while (useMultiLine)
                {
                    if (prompt.ToUpper() == "[END]")
                    {
                        useMultiLine = false;
                        prompt = multiLineContent.ToString();
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("请继续输入，直到输入 [END] 停止...");
                        prompt = Console.ReadLine();
                        multiLineContent.Append(prompt);
                    }
                }

                var dt = SystemTime.Now;


                var request = iWantToRun.CreateRequest(prompt, true);
                var result = await iWantToRun.RunAsync(request);

                await Console.Out.WriteLineAsync("回复：");
                await Console.Out.WriteLineAsync(result.Output);
                await Console.Out.WriteLineAsync();
            }
        }
    }
}