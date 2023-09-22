﻿using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public class ChatSample
    {
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public ChatSample(IAiHandler aiHandler)
        {
            _aiHandler = aiHandler;
        }


        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync(@"ChatSample 开始运行，请输入对话内容。
输入 [ML] 开启单次对话的多行模式
输入 [END] 完成所有多行输入
输入 exit 退出。");

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var chatConfig = _semanticAiHandler.ChatConfig(parameter, userId: "Jeffrey",modelName: SampleHelper.Default_TextCompletion/*, modelName: "gpt-4-32k"*/);
            var iWantToRun = chatConfig.iWantToRun;

            var multiLineContent = new StringBuilder();
            var useMultiLine = false;
            //开始对话
            while (true)
            {
                await Console.Out.WriteLineAsync("人类：");
                var prompt = Console.ReadLine();

                if (prompt.ToUpper() == "[ML]")
                {
                    await Console.Out.WriteLineAsync("识别到多行模式，请继续输入");
                    useMultiLine = true;
                }

                while (useMultiLine)
                {
                    if (prompt.ToUpper() == "[END]")
                    {
                        useMultiLine = false;
                        prompt  = multiLineContent.ToString();
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("请继续输入，直到输入 [END] 停止...");
                        prompt = Console.ReadLine();
                        multiLineContent.Append(prompt);
                    }
                }


                if (prompt == "exit")
                {
                    break;
                }

                var dt = SystemTime.Now;
                var result = await _semanticAiHandler.ChatAsync(iWantToRun, prompt);

                await Console.Out.WriteLineAsync("机器：");
                await Console.Out.WriteLineAsync(result.Output);
                await Console.Out.WriteLineAsync();
            }
        }
    }
}
