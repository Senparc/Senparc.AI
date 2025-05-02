﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Senparc.CO2NET.Extensions;
using Microsoft.Extensions.Http.Logging;
using System.Net.Http;
//using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System.Net;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Runtime.CompilerServices;
using Senparc.AI.Entities.Keys;
using Senparc.CO2NET;
using RTools_NTS.Util;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

// Memory functionality is experimental
#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020, SKEXP0012, SKEXP0001

namespace Senparc.AI.Kernel.Helpers
{

    public partial class SemanticKernelHelper
    {
        /* Config* 方法规则：
        1. 相关方法为较底层的调用方法，会直接使用 Semantic Kernel 等模块接口
        2. 所有 modelName、deploymentName，都是用字符串传入，如果留空，则使用 SenparcAiSetting 参数自动获取。
       */



        #region Config


        /// <summary>
        /// 设置 Kernel，并配置 TextCompletion 模型
        /// </summary>
        /// <param name="userId">用户ID， 用于防止api滥用</param>
        /// <param name="modelName">模型名 modelId</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IKernelBuilder ConfigChat(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null,
            IKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
            modelName ??= senparcAiSetting.ModelName.Chat;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法
            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(modelName,
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        httpClient: _httpClient),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.AzureEndpoint,
                        apiKey: senparcAiSetting.ApiKey,
                        httpClient: _httpClient),
                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.NeuCharEndpoint,
                        apiKey: senparcAiSetting.ApiKey,
                        httpClient: _httpClient),
                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextGeneration(
                        model: modelName,
                        apiKey: null,
                        endpoint: new Uri(senparcAiSetting.HuggingFaceEndpoint ?? throw new SenparcAiException("HuggingFace 必须提供 Endpoint")),
                        serviceId: serviceId,
                        httpClient: _httpClient),
                AiPlatform.FastAPI => kernelBuilder.AddFastAPIChatCompletion(
                        modelId: modelName,
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        endpoint: senparcAiSetting.FastAPIEndpoint,
                        serviceId: null
                    ),
                AiPlatform.Ollama => kernelBuilder.AddOllamaChatCompletion(
                        modelId: modelName,
                        endpoint: new Uri(senparcAiSetting.OllamaEndpoint),
                        serviceId: null
                    ),
                //DeepSeek 使用和 OpenAI 一致的请求格式
                AiPlatform.DeepSeek => kernelBuilder.AddOpenAIChatCompletion(modelName,
                        endpoint: new Uri(senparcAiSetting.Endpoint),
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        httpClient: _httpClient),

                _ => throw new SenparcAiException($"ConfigChat 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// 设置 Kernel，并配置 TextCompletion 模型
        /// </summary>
        /// <param name="userId">用户ID， 用于防止api滥用</param>
        /// <param name="modelName">模型名 modelId</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IKernelBuilder ConfigTextCompletion(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null,
            IKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
            modelName ??= senparcAiSetting.ModelName.TextCompletion;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;

            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default

            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(modelName,
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        httpClient: _httpClient),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.Endpoint,
                        apiKey: senparcAiSetting.ApiKey,
                        httpClient: _httpClient),
                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.Endpoint,
                        apiKey: senparcAiSetting.ApiKey,
                        httpClient: _httpClient),
                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextGeneration(
                        model: modelName,
                        apiKey: null,
                        endpoint: new Uri(senparcAiSetting.Endpoint ?? throw new SenparcAiException("HuggingFace 必须提供 Endpoint")),
                        serviceId: null,
                        httpClient: _httpClient),
                AiPlatform.FastAPI => kernelBuilder.AddFastAPIChatCompletion(
                        modelId: modelName,
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        endpoint: senparcAiSetting.FastAPIEndpoint,
                        serviceId: null
                    ),
                AiPlatform.Ollama => kernelBuilder.AddOllamaTextGeneration(
                        modelId: modelName,
                        endpoint: new Uri(senparcAiSetting.OllamaEndpoint),
                        serviceId: null
                    ),
                AiPlatform.DeepSeek => kernelBuilder.AddOpenAIChatCompletion(modelName,
                        endpoint: new Uri(senparcAiSetting.Endpoint),
                        apiKey: senparcAiSetting.ApiKey,
                        orgId: senparcAiSetting.OrganizationId,
                        httpClient: _httpClient),
                _ => throw new SenparcAiException($"ConfigTextCompletion 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// 设置 Kernel，并配置 TextCompletion 模型
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IKernelBuilder ConfigTextEmbeddingGeneration(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null, IKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            //kernel ??= GetKernel();

            senparcAiSetting ??= this.AiSetting;
            modelName ??= senparcAiSetting.ModelName.Embedding;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;
            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            //TODO：Builder 不应该新建

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法
            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextEmbeddingGeneration(
                    modelId: modelName,
                    apiKey: senparcAiSetting.ApiKey,
                    orgId: senparcAiSetting.OrganizationId,
                    httpClient: _httpClient),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: deploymentName,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    modelId: modelName,
                    httpClient: _httpClient),

                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: deploymentName,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    modelId: modelName,
                    httpClient: _httpClient),

                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextEmbeddingGeneration(
                    model: modelName,
                    endpoint: new Uri(senparcAiSetting.Endpoint ?? throw new SenparcAiException("HuggingFace 必须提供 Endpoint")),
                    httpClient: _httpClient),

                AiPlatform.Ollama => kernelBuilder.AddOllamaTextEmbeddingGeneration(
                    modelId: modelName,
                    endpoint: new Uri(senparcAiSetting.OllamaEndpoint)),

                _ => throw new SenparcAiException($"ConfigTextEmbeddingGeneration 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            //TODO:测试多次添加
            //KernelBuilder = builder;
            return kernelBuilder;
        }

        /// <summary>
        /// 设置 DallE 接口，默认强制使用 OpenAI 权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kernelBuilder"></param>
        /// <param name="azureModeId">AzureOpenAI 的模型名称</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="azureDallEDepploymentName">AzureAI 的 DallE 模型部署名称</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public IKernelBuilder ConfigImageGeneration(string userId, IKernelBuilder? kernelBuilder = null, string azureModeId = null, ISenparcAiSetting senparcAiSetting = null, string azureDallEDepploymentName = null)
        {
            senparcAiSetting ??= this.AiSetting;

            var serviceId = GetServiceId(userId, "image-generation");
            var aiPlatForm = senparcAiSetting.AiPlatform;

            //TODO：Builder 不应该新建
            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextToImage(
                    apiKey: senparcAiSetting.ApiKey,
                    orgId: senparcAiSetting.OrganizationId,
                    httpClient: _httpClient),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextToImage(
                    deploymentName: azureDallEDepploymentName,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    modelId: azureModeId,
                    httpClient: _httpClient),

                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAITextToImage(
                    deploymentName: azureDallEDepploymentName,
                    endpoint: senparcAiSetting.Endpoint,
                    apiKey: senparcAiSetting.ApiKey,
                    modelId: azureModeId,
                    httpClient: _httpClient),

                _ => throw new SenparcAiException($"ConfigImageGeneration 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Get Embedding Result
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="text"></param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="azureDeployName"></param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string modelName, string text, ISenparcAiSetting senparcAiSetting = null, string azureDeployName = null)
        {
            senparcAiSetting ??= this.AiSetting;
            var aiPlatForm = senparcAiSetting.AiPlatform;

            var embeddingService = aiPlatForm switch
            {
                //AiPlatform.OpenAI =>  new OpenAITextEmbeddingGenerationService(
                //                apiKey: senparcAiSetting.ApiKey,
                //                httpClient: _httpClient,
                //                modelId: modelName,
                //                loggerFactory: loggerFactory
                //           );
                // ),

                //memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(
                //    modelId: modelName,
                //    apiKey: senparcAiSetting.ApiKey,
                //    orgId: senparcAiSetting.OrganizationId,
                //    httpClient: _httpClient),
                AiPlatform.AzureOpenAI => new AzureOpenAITextEmbeddingGenerationService(
                           deploymentName: azureDeployName,
                           endpoint: senparcAiSetting.Endpoint,
                                apiKey: senparcAiSetting.ApiKey,
                           httpClient: _httpClient,
                                modelId: modelName,
                                loggerFactory: loggerFactory
                           ),
                //AiPlatform.HuggingFace => memoryBuilder.WithTextEmbeddingGeneration(
                //    (loggerFactory, httpClient) =>
                //    {
                //        return new AzureOpenAITextEmbeddingGenerationService(
                //        deploymentName: azureDeployName,
                //             endpoint: senparcAiSetting.Endpoint,
                //             apiKey: senparcAiSetting.ApiKey,
                //        httpClient: _httpClient,
                //             modelId: modelName,
                //             loggerFactory: loggerFactory
                //        );
                //    }),

                //AiPlatform.Ollama => memoryBuilder.WithTextEmbeddingGeneration((loggerFactory, httpClient) =>
                //{
                //    return new OllamaTextEmbeddingGenerationService(
                //         endpoint: new Uri(senparcAiSetting.Endpoint),
                //         modelId: modelName,
                //         loggerFactory: loggerFactory
                //    );
                //}),

                _ => throw new SenparcAiException($"GetEmbedding 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            var embeddingResult = await embeddingService.GenerateEmbeddingAsync(text, _kernel);

            return embeddingResult;

        }

        #region Memory 相关

        ISemanticTextMemory _textMemory = null;//TODO:适配多重不同的请求

        /// <summary>
        /// 尝试获取 ISemanticTextMemory 对象
        /// </summary>
        /// <returns></returns>
        /// <exception cref="SenparcAiException">当 ISemanticTextMemory 未设置时抛出</exception>
        public ISemanticTextMemory? TryGetMemory()
        {
            if (_textMemory == null)
            {
                throw new SenparcAiException("_textMemory 未设置！");
            }
            return _textMemory;
        }

        /// <summary>
        /// 获取 ISemanticTextMemory 对象
        /// </summary>
        /// <returns></returns>
        //[Obsolete("该方法已被SK放弃，原文为：Memory functionality will be placed in separate Microsoft.SemanticKernel.Plugins.Memory package. This will be removed in a future release. See sample dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs in the semantic-kernel repository.")]
        [Obsolete]
        public ISemanticTextMemory? GetMemory(string modelName, ISenparcAiSetting senparcAiSetting,
            IKernelBuilder? kernelBuilder, string azureDeployName = null, ITextEmbeddingGenerationService textEmbeddingGeneration = null)
        {
            if (_textMemory == null)
            {
                senparcAiSetting ??= this.AiSetting;
                var aiPlatForm = senparcAiSetting.AiPlatform;

                var memoryBuilder = new MemoryBuilder();
                memoryBuilder.WithHttpClient(_httpClient);



                _ = aiPlatForm switch
                {
                    AiPlatform.OpenAI => memoryBuilder.WithTextEmbeddingGeneration(
                           (loggerFactory, httpClient) =>
                           {
                               return new OpenAITextEmbeddingGenerationService(
                                    apiKey: senparcAiSetting.ApiKey,
                                    httpClient: _httpClient,
                                    modelId: modelName,
                                    loggerFactory: loggerFactory
                               );
                           }
                     ),

                    //memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(
                    //    modelId: modelName,
                    //    apiKey: senparcAiSetting.ApiKey,
                    //    orgId: senparcAiSetting.OrganizationId,
                    //    httpClient: _httpClient),
                    AiPlatform.AzureOpenAI => memoryBuilder.WithTextEmbeddingGeneration(
                           (loggerFactory, httpClient) =>
                           {
                               return new AzureOpenAITextEmbeddingGenerationService(
                                    deploymentName: azureDeployName,
                                    endpoint: senparcAiSetting.Endpoint,
                                    apiKey: senparcAiSetting.ApiKey,
                                    httpClient: _httpClient,
                                    modelId: modelName,
                                    loggerFactory: loggerFactory
                               );
                           }
                    ),

                    AiPlatform.HuggingFace => memoryBuilder.WithTextEmbeddingGeneration(
                        (loggerFactory, httpClient) =>
                        {
                            return new AzureOpenAITextEmbeddingGenerationService(
                                 deploymentName: azureDeployName,
                                 endpoint: senparcAiSetting.Endpoint,
                                 apiKey: senparcAiSetting.ApiKey,
                                 httpClient: _httpClient,
                                 modelId: modelName,
                                 loggerFactory: loggerFactory
                            );
                        }),

                    AiPlatform.Ollama => memoryBuilder.WithTextEmbeddingGeneration((loggerFactory, httpClient) =>
                    {
                        return new OllamaTextEmbeddingGenerationService(
                             endpoint: new Uri(senparcAiSetting.Endpoint),
                             modelId: modelName,
                             loggerFactory: loggerFactory
                        );
                    }),

                    _ => throw new SenparcAiException($"GetMemory 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
                };


                memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
                //.WithMemoryStore(new AzureAISearchMemoryStore(senparcAiSetting.AzureEndpoint, senparcAiSetting.ApiKey))

                _textMemory = memoryBuilder.Build();
            }


            return _textMemory;
        }

        #endregion


        /// <summary>
        /// Save some information into the semantic memory, keeping only a reference to the source information.
        /// </summary>
        /// <param name="memory">ISemanticTextMemory 对象</param>
        /// <param name="collection">Collection where to save the information</param>
        /// <param name="text">Information to save</param>
        /// <param name="externalId">Unique identifier, e.g. URL or GUID to the original source</param>
        /// <param name="externalSourceName">Name of the external service, e.g. "MSTeams", "GitHub", "WebSite", "Outlook IMAP", etc.</param>
        /// <param name="description">Optional description</param>
        /// <param name="additionalMetadata"></param>
        /// <param name="kernel">Kernel</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns></returns>
        public async Task MemorySaveReferenceAsync(ISemanticTextMemory memory,
            string collection,
            string text,
            string externalId,
            string externalSourceName,
            string? description = null,
            string? additionalMetadata = null,
            Microsoft.SemanticKernel.Kernel? kernel = null,
            CancellationToken cancel = default)
        {
            await memory.SaveReferenceAsync(collection, text, externalId, externalSourceName, description,
                additionalMetadata, kernel ?? GetKernel(), cancel);
        }

        /// <summary>
        /// Save some information into the semantic memory, keeping a copy of the source information.
        /// </summary>
        /// <param name="memory">ISemanticTextMemory 对象</param>
        /// <param name="collection">Collection where to save the information</param>
        /// <param name="id">Unique identifier</param>
        /// <param name="text">Information to save</param>
        /// <param name="description">Optional description</param>
        /// <param name="additionalMetadata"></param>
        /// <param name="kernel">Kernel</param>
        /// <param name="cancel">Cancellation token</param>        /// <returns></returns>
        public async Task MemorySaveInformationAsync(ISemanticTextMemory memory,
            string collection,
            string text,
            string id,
            string? description = null,
            string? additionalMetadata = null,
            Microsoft.SemanticKernel.Kernel? kernel = null,
            CancellationToken cancel = default)
        {
            await memory.SaveInformationAsync(collection, text, id, description, additionalMetadata, kernel ?? GetKernel(), cancel);
        }

        /// <summary>
        /// 添加 Memory 操作
        /// </summary>
        /// <param name="task"></param>
        public void AddMemory(Task task)
        {
            _memoryExecuteList.Add(task);
        }

        /// <summary>
        /// 执行 Memory 操作，并等待
        /// </summary>
        public void ExecuteMemory()
        {
            foreach (var task in _memoryExecuteList)
            {
                Task.Run(() => task);
            }

            Task.WaitAll(_memoryExecuteList.ToArray());
            _memoryExecuteList.Clear();
        }

        #endregion
    }
}

#pragma warning restore SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020

