﻿using Azure;
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

            vectorMemory = new KernelMemoryBuilder()
                .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig()
                {
                    APIKey = aiSetting.ApiKey,
                    APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                    Deployment =aiSetting.ModelName.Embedding, //aiSetting.DeploymentName,
                    Endpoint = aiSetting.Endpoint,
                    Auth = AzureOpenAIConfig.AuthTypes.APIKey,

                })
                      .WithRedisMemoryDb(Senparc.CO2NET.Config.SenparcSetting.Cache_Redis_Configuration)
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


                if (isReference)
                {
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
                }
                else
                {
                    iWantToRun
                    .MemorySaveInformation(
                        modelName: textEmbeddingGenerationName(aiSetting),
                        azureDeployName: textEmbeddingAzureDeployName(aiSetting),
                        collection: memoryCollectionName, id: info[0], text: info[1]);


                    var tags = new TagCollection();
                    tags.Add($"Senparc-{info[0]}", $"Senparc-{info[1]}");

                    await vectorMemory.ImportTextAsync(info[1], "Senparc.AI", tags, info[0]);
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
                var vectorResult = await vectorMemory.SearchAsync(question, null, null, null, 1, limit);
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
