using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles
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
            await Console.Out.WriteLineAsync("ChatSample 开始运行，请输入对话内容，输入 exit 退出。");

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var chatConfig = _semanticAiHandler.ChatConfig(parameter, userId: "Jeffrey");
            var iWantToRun = chatConfig.iWantToRun;

            //开始对话
            while (true)
            {
                await Console.Out.WriteLineAsync("人类：");
                var prompt = Console.ReadLine();
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
