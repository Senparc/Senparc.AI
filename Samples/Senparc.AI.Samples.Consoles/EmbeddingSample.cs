using Microsoft.SemanticKernel.Memory;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using System;
using System.Collections.Concurrent;
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
        string _userId = "Jeffrey";
        string memoryCollectionName = "EmbeddingTest";

        public EmbeddingSample(IAiHandler aiHandler)
        {
            _aiHandler = aiHandler;
        }

        public async Task RunAsync()
        {
            await Console.Out.WriteLineAsync("EmbeddingSample 开始运行。请输入需要 Embedding 的内容，id 和 text 以 :::（三个引文冒号）分割，输入 n 继续下一步。");

            await Console.Out.WriteLineAsync("请输入");

            //测试 TextEmbedding
            var iWantToRun = _semanticAiHandler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, _userId, "text-embedding-ada-002")
                 .ConfigModel(ConfigModel.TextCompletion, _userId, "text-davinci-003")
                 .BuildKernel(b => b.WithMemoryStorage(new VolatileMemoryStore()));


            //开始对话
            while (true)
            {
                var prompt = Console.ReadLine();
                if (prompt == "n")
                {
                    break;
                }

                var info = prompt.Split(new[] { ":::" }, StringSplitOptions.None);

                iWantToRun
                    .MemorySaveInformation(memoryCollectionName, id: info[0], text: info[1]);
            }

            iWantToRun.MemoryStoreExexute();


            while (true)
            {
                await Console.Out.WriteLineAsync("请提问：");
                var question = Console.ReadLine();

                var questionDt = DateTime.Now;
                var result = await iWantToRun.MemorySearchAsync(memoryCollectionName, question);
                var response = result.MemoryQueryResult.FirstOrDefaultAsync();
                Console.WriteLine("应答： " + response.Result?.Metadata.Text + $"\r\n -- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                Console.WriteLine();
            }
          
        }

    }
}
