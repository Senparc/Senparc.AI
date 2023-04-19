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
    public class EmbeddingSample
    {
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public EmbeddingSample(IAiHandler aiHandler)
        {
            _aiHandler = aiHandler;
        }

        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync("EmbeddingSample 开始运行。请输入需要 Embedding 的内容，Text 和 Description 以 :::（三个引文冒号）分割，输入 n 继续下一步。");

            await Console.Out.WriteLineAsync("请输入");

            //开始对话
            while (true)
            {
                var prompt = Console.ReadLine();
                if (prompt == "n")
                {
                    break;
                }

                var info = prompt.Split(new[] { ":::"}, StringSplitOptions.None);

                //iWantToRun
                //  .MemorySaveInformation(MemoryCollectionName, id: "info1", text: "My name is Andrea")
            }

        }

    }
}
