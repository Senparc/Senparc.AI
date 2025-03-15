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
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public partial class EmbeddingSample
    {
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;
        string _userId = "Jeffrey";
        string memoryCollectionName = "EmbeddingTest";
        Func<ISenparcAiSetting, string> textEmbeddingGenerationName => setting => setting.ModelName.Embedding ?? "text-embedding-ada-002";
        Func<ISenparcAiSetting, string> textEmbeddingAzureDeployName => setting => setting.ModelName.Embedding ?? "text-embedding-ada-002";

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

            //测试 TextEmbedding
            var iWantToRun = _semanticAiHandler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, _userId)
                 .ConfigModel(ConfigModel.TextCompletion, _userId)
                 .BuildKernel();


            //.BuildKernel(b => b.WithMemoryStorage(new VolatileMemoryStore()));
            var aiSetting = iWantToRun.SemanticKernelHelper.AiSetting;
            IKernelMemory vectorMemory;
            var openAIConfigEmbedding = new AzureOpenAIConfig()
            {
                APIKey = aiSetting.ApiKey,
                APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                Deployment = aiSetting.ModelName.Embedding, //aiSetting.DeploymentName,
                Endpoint = aiSetting.Endpoint,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                MaxEmbeddingBatchSize = 1,
                MaxRetries = 2,
                MaxTokenTotal = 1000
            };
            var azureOpenAIConfigChat = new AzureOpenAIConfig()
            {
                APIKey = aiSetting.ApiKey,
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Deployment = aiSetting.ModelName.Chat, //aiSetting.DeploymentName,
                Endpoint = aiSetting.Endpoint,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                MaxEmbeddingBatchSize = 1,
                MaxRetries = 2,
                MaxTokenTotal = 1000
            };

            var openAIConfigText = new OpenAIConfig()
            {
                APIKey = aiSetting.ApiKey,
                TextModel = aiSetting.ModelName.Chat,
                EmbeddingModel = aiSetting.ModelName.Embedding,
                EmbeddingModelMaxTokenTotal = 2048,
                MaxEmbeddingBatchSize = 1,
                MaxRetries = 3,
                Endpoint = aiSetting.Endpoint,
            };

            var simpleVectorDbConfig = new SimpleVectorDbConfig()
            {
                StorageType = FileSystemTypes.Disk
            };

            vectorMemory = new KernelMemoryBuilder()
                .WithAzureOpenAITextEmbeddingGeneration(openAIConfigEmbedding)
                .WithOpenAITextGeneration(openAIConfigText)
                .WithSimpleVectorDb(simpleVectorDbConfig)
                //.WithRedisMemoryDb(Senparc.CO2NET.Config.SenparcSetting.Cache_Redis_Configuration)
                //.WithOpenAIDefaults(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
                .Build<MemoryServerless>();

            

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

                switch (aiSetting.VectorDB.Type)
                {
                    case VectorDB.VectorDBType.HardDisk:
                        {
                            var tags = new TagCollection();
                            tags.Add($"Senparc{info[0].ToString().Trim().Replace(":", "")}", $"Senparc{info[1].ToString().Trim().Replace(":", "")}");

                            await vectorMemory.ImportTextAsync(info[1], "SenparcAI", tags, info[0]);
                            break;
                        }
                    case VectorDB.VectorDBType.Redis:
                        {
                            string strKey = $"Senparc{info[0].ToString().Trim().Replace(":", "")}";
                            string strValue = $"Senparc{info[1].ToString().Trim().Replace(":", "")}";

                            List<Dictionary<string, char?>> lstValues = new List<Dictionary<string, char?>>();
                            Dictionary<string, char?> keyValuePairs = new Dictionary<string, char?>();
                            keyValuePairs.Add(strKey, 'a');
                            lstValues.Add(keyValuePairs);
                            var tags = new Dictionary<string, char?> { { "__part_n", ',' }, { "collection", ',' } };
                            var redisConfig = new RedisConfig("km-", tags);
                            redisConfig.ConnectionString = Senparc.CO2NET.Config.SenparcSetting.Cache_Redis_Configuration;

                            redisConfig.VectorAlgorithm = new NRedisStack.Search.Schema.VectorField.VectorAlgo();

                            //await vectorMemory.ImportTextAsync(info[1], "SenparcAI", tags, info[0]);
                            break;
                        }
                    case VectorDB.VectorDBType.Default:
                        {
                            //内存
                            iWantToRun.MemorySaveReference(
                                 modelName: textEmbeddingGenerationName(aiSetting),
                                 azureDeployName: textEmbeddingAzureDeployName(aiSetting),
                                 collection: memoryCollectionName,
                                 description: info[1],//只用于展示记录
                                 text: info[1],//真正用于生成 embedding
                                 externalId: info[0],
                                 externalSourceName: memoryCollectionName
                                );
                            await Console.Out.WriteLineAsync($"  URL {i + 1} saved");
                            break;
                        }
                    default:
                        {
                            iWantToRun
                            .MemorySaveInformation(
                                modelName: textEmbeddingGenerationName(aiSetting),
                                azureDeployName: textEmbeddingAzureDeployName(aiSetting),
                                collection: memoryCollectionName, id: info[0], text: info[1]);


                            //TagCollection tags = new TagCollection();
                            //tags.Add($"Senparc-{info[0]}", $"Senparc-{info[1]}");

                            //await vectorMemory.ImportTextAsync(info[1], "Senparc.AI", tags, info[0]);
                            break;
                        }
                }
                i++;
            }

            iWantToRun.MemoryStoreExexute();

            while (true)
            {
                await Console.Out.WriteLineAsync("请提问：");
                var question = Console.ReadLine();
                if (question == "exit")
                {
                    break;
                }

                var questionDt = DateTime.Now;
                var limit = isReference ? 3 : 2;

                //新方
                var vectorResult = await vectorMemory.SearchAsync(question, null, null, null, 0.1, limit);
                foreach (var restulItem in vectorResult.Results)
                {
                    Console.WriteLine("新结果：" + restulItem.ToJson(true));
                }


                //老方法
                var result = await iWantToRun.MemorySearchAsync(
                        modelName: textEmbeddingGenerationName(aiSetting),
                        azureDeployName: textEmbeddingAzureDeployName(aiSetting),
                        memoryCollectionName: memoryCollectionName,
                        query: question,
                        limit: limit,
                        minRelevanceScore: 0.6);

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
