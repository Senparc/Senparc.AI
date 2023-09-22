using Microsoft.SemanticKernel.Memory;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;

namespace Senparc.AI.Samples.Consoles.Samples
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

        public async Task RunAsync(bool isReference = false)
        {
            if (isReference)
            {
                await Console.Out.WriteLineAsync("EmbeddingSample 开始运行。请输入需要 Embedding 的内容，id 和 text 以 :::（三个英文冒号）分割，输入 n 继续下一步。");
            }
            else
            {
                await Console.Out.WriteLineAsync("EmbeddingSample 开始运行。请输入需要 Embedding 的内容，URL 和介绍以 :::（三个英文冒号）分割，输入 n 继续下一步。");
            }

            await Console.Out.WriteLineAsync("请输入");

            //测试 TextEmbedding
            var iWantToRun = _semanticAiHandler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, _userId, SampleHelper.Default_TextEmbedding_ModeName)
                 .ConfigModel(ConfigModel.TextCompletion, _userId, SampleHelper.Default_TextCompletion_ModeName)
                 .BuildKernel(b => b.WithMemoryStorage(new VolatileMemoryStore()));

            //开始对话
            var i = 0;
            while (true)
            {
                var prompt = Console.ReadLine();
                if (prompt == "n")
                {
                    break;
                }

                var info = prompt.Split(new[] { ":::" }, StringSplitOptions.None);

                if (isReference)
                {
                    iWantToRun.MemorySaveReference(
                         collection: memoryCollectionName,
                         description: info[1],//只用于展示记录
                         text: info[1],//真正用于生成 embedding
                         externalId: info[0],
                         externalSourceName: memoryCollectionName
                        );
                    await Console.Out.WriteLineAsync($"  URL {i + 1} saved");
                }
                else
                {

                    iWantToRun
                    .MemorySaveInformation(memoryCollectionName, id: info[0], text: info[1]);
                }
                i++;
            }

            iWantToRun.MemoryStoreExexute();


            while (true)
            {
                await Console.Out.WriteLineAsync("请提问：");
                var question = Console.ReadLine();

                var questionDt = DateTime.Now;
                var limit = isReference ? 3 : 1;
                var result = await iWantToRun.MemorySearchAsync(memoryCollectionName, question, limit);

                var j = 0;
                if (isReference)
                {
                    await foreach (var item in result.MemoryQueryResult)
                    {
                        await Console.Out.WriteLineAsync($"应答结果[{j + 1}]：");
                        await Console.Out.WriteLineAsync("  URL:\t\t" + item.Metadata.Id?.Trim());
                        await Console.Out.WriteLineAsync("  Description:\t" + item.Metadata.Description);
                        await Console.Out.WriteLineAsync("  Text:\t\t" + item.Metadata.Text);
                        await Console.Out.WriteLineAsync("  Relevance:\t" + item.Relevance);
                        await Console.Out.WriteLineAsync($"-- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                        j++;
                    }
                }
                else
                {
                    await foreach (var item in result.MemoryQueryResult)
                    {
                        var response = item;
                        if (response != null)
                        {
                            j++;
                        }
                        else
                        {
                            continue;
                        }

                        await Console.Out.WriteLineAsync($"应答[{j + 1}]： " + response.Metadata.Text +
                            $"\r\n -- Relevance {response.Relevance} -- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                    }
                }

                if (j == 0)
                {
                    await Console.Out.WriteLineAsync("无匹配结果");
                }

                await Console.Out.WriteLineAsync();

            }

        }

    }
}
