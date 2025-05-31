using Azure;
using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using System.Reflection.Metadata;
using System.Text;
using Senparc.Xncf.SenMapic.Domain.SiteMap;
using Senparc.CO2NET.Helpers;
using Microsoft.Extensions.VectorData;

namespace Senparc.AI.Samples.Consoles.Samples
{

    public class Record
    {
        [VectorStoreKey]
        public ulong Id { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string Name { get; set; }

        [VectorStoreData(IsFullTextIndexed = true)]
        public string Description { get; set; }

        [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
        public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string[] Tags { get; set; }
    }

    public partial class EmbeddingSample
    {
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;
        string _userId = "Jeffrey";
        string memoryCollectionName = "EmbeddingTest";
        static Func<ISenparcAiSetting, string> textEmbeddingGenerationName => setting => setting.ModelName.Embedding ?? "text-embedding-ada-002";
        static Func<ISenparcAiSetting, string> textEmbeddingAzureDeployName => setting => setting.ModelName.Embedding ?? "text-embedding-ada-002";

        public EmbeddingSample(IAiHandler aiHandler)
        {
            _aiHandler = aiHandler;
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//同步日志设置状态
        }

        public async Task RunAsync(bool isReference = false, bool isRag = false)
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


            var aiSetting = _semanticAiHandler.SemanticKernelHelper.AiSetting;
            var vectorName = "senparc-vector-record-ai";

            //测试 TextEmbedding
            var iWantToRun = _semanticAiHandler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, _userId)
                 .ConfigModel(ConfigModel.TextCompletion, _userId)
                 .ConfigVectorStore(aiSetting.VectorDB)
                 .BuildKernel();

            var vectorCollection = iWantToRun.GetVectorCollection<ulong, Record>(aiSetting.VectorDB, vectorName);
            await vectorCollection.EnsureCollectionExistsAsync();

            var modelName = textEmbeddingGenerationName(aiSetting);

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

                var record = new Record()
                {
                    Id = ulong.Parse(info[0]),
                    Name = vectorName + ":" + info[0],
                    Description = info[1],
                    DescriptionEmbedding = await iWantToRun.SemanticKernelHelper.GetEmbeddingAsync(modelName, info[1]),
                    Tags = new[] { info[0] }
                };
                await vectorCollection.UpsertAsync(record);

                i++;
            }

            //iWantToRun.MemoryStoreExexute();

            while (true)
            {
                await Console.Out.WriteLineAsync("请提问：");
                String question = Console.ReadLine();
                if (question == "exit")
                {
                    break;
                }

                var questionDt = DateTime.Now;
                var top = 3;// isReference ? 3 : 2;

                ReadOnlyMemory<float> searchVector = await iWantToRun.SemanticKernelHelper.GetEmbeddingAsync(modelName, question);

                //var r1 = await vectorCollection.GetAsync(1);//OK
                //Console.WriteLine("r1:" + r1.ToJson(true));

                //vectorCollection = iWantToRun.GetVectorCollection<string, Record>(aiSetting.VectorDB, "senparc-vector-record");
                var vectorResult = vectorCollection.SearchAsync(searchVector, top);
                var j = 0;
                await foreach (var restulItem in vectorResult)
                {
                    await Console.Out.WriteLineAsync($"应答结果[{j + 1}]：");
                    await Console.Out.WriteLineAsync("  Id:\t\t" + restulItem.Record.Id);
                    await Console.Out.WriteLineAsync("  Description:\t\t" + restulItem.Record.Description);
                    await Console.Out.WriteLineAsync("  Id:\t\t" + string.Join(',', restulItem.Record.Tags));
                    await Console.Out.WriteLineAsync("  Relevance:\t\t" + restulItem.Score);
                    await Console.Out.WriteLineAsync($"-- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                    j++;
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
