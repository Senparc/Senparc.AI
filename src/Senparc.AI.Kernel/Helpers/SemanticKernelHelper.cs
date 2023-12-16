using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.CO2NET;

// Memory functionality is experimental
#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020

namespace Senparc.AI.Kernel.Helpers
{
    /// <summary>
    /// SemanticKernel 帮助类
    /// </summary>
    public class SemanticKernelHelper
    {
        public ISemanticTextMemory? SemanticTextMemory { get; set; }

        private Microsoft.SemanticKernel.Kernel _kernel { get; set; }

        internal KernelBuilder KernelBuilder { get; set; } = new KernelBuilder();

        internal ISenparcAiSetting AiSetting { get; }

        private List<Task> _memoryExecuteList = new List<Task>();
        private readonly ILoggerFactory? loggerFactory;


        public SemanticKernelHelper(ISenparcAiSetting? aiSetting = null, ILoggerFactory? loggerFactory = null)
        {
            AiSetting = aiSetting ?? Senparc.AI.Config.SenparcAiSetting;
            this.loggerFactory = loggerFactory;
        }

        /// <summary>
        /// 获取对话 ServiceId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public string GetServiceId(string userId, string modelName)
        {
            return $"{userId}-{modelName}";
        }

        /// <summary>
        /// 获取 SemanticKernel 对象
        /// </summary>
        /// <param name="kernelBuilderAction"><see cref="KernelBuilder"/> 在进行 <see cref="KernelBuilder.Build()"/> 之前需要插入的操作</param>
        /// <param name="refresh" default="false">是否需要刷新kernel</param>
        /// <returns></returns>
        public Microsoft.SemanticKernel.Kernel GetKernel(Action<KernelBuilder>? kernelBuilderAction = null, bool refresh = false)
        {
            if (_kernel != null && !refresh)
            {
                return _kernel;
            }

            return BuildKernel(KernelBuilder, kernelBuilderAction);
        }


        /// <summary>
        /// Build 新的 Kernel 对象
        /// </summary>
        /// <param name="kernelBuilder"></param>
        /// <param name="kernelBuilderAction"></param>
        /// <returns></returns>
        public Microsoft.SemanticKernel.Kernel BuildKernel(KernelBuilder kernelBuilder, Action<KernelBuilder>? kernelBuilderAction = null)
        {
            kernelBuilderAction?.Invoke(kernelBuilder);

            if (loggerFactory != null)
            {
                kernelBuilder.Services.AddSingleton(loggerFactory);
            }

            _kernel = kernelBuilder.Build();
            return _kernel;
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
        public KernelBuilder ConfigTextCompletion(string userId, string modelName, ISenparcAiSetting senparcAiSetting,
            KernelBuilder? kernelBuilder, string azureDeployName = null)
        {
            var serviceId = GetServiceId(userId, modelName);
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
            var aiPlatForm = senparcAiSetting.AiPlatform;

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法
            kernelBuilder ??= new KernelBuilder();

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(modelName, apiKey: senparcAiSetting.ApiKey,
                    orgId: senparcAiSetting.OrganizationId),
                AiPlatform.AzureOpenAI =>
                    // kernelBuilder.WithAzureTextCompletionService(modelName, AiSetting.AzureEndpoint, AiSetting.ApiKey, AiSetting.AzureOpenAIApiVersion),
                    kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: azureDeployName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.AzureEndpoint,
                        apiKey: senparcAiSetting.ApiKey),
                AiPlatform.NeuCharOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                        deploymentName: azureDeployName,
                        modelId: modelName,
                        endpoint: senparcAiSetting.NeuCharEndpoint,
                        apiKey: senparcAiSetting.ApiKey),
                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextGeneration(
                        model: modelName,
                        endpoint: senparcAiSetting.HuggingFaceEndpoint),
                _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// 设置 Kernel，并配置 TextCompletion 模型
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public KernelBuilder ConfigTextEmbeddingGeneration(string userId, string modelName, KernelBuilder? kernelBuilder = null)
        {
            //kernel ??= GetKernel();

            var serviceId = GetServiceId(userId, modelName);
            var senparcAiSetting = Senparc.AI.Config.SenparcAiSetting;
            var aiPlatForm = AiSetting.AiPlatform;

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            //TODO：Builder 不应该新建

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法
            kernelBuilder ??= new KernelBuilder();

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextEmbeddingGeneration(modelName, senparcAiSetting.ApiKey,
                    AiSetting.OrganizationId),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(modelName,
                    senparcAiSetting.AzureEndpoint, senparcAiSetting.ApiKey, senparcAiSetting.AzureOpenAIApiVersion),

                AiPlatform.NeuCharOpenAI => kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(modelName,
                    senparcAiSetting.NeuCharEndpoint, senparcAiSetting.ApiKey, senparcAiSetting.AzureOpenAIApiVersion),

                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextEmbeddingGeneration(modelName,
                    senparcAiSetting.HuggingFaceEndpoint),

                _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            //kernelBuilder.Configure(c =>
            //{
            //    c.AddTextEmbeddingGenerationService(serviceId, k =>
            //        aiPlatForm switch
            //        {
            //            AiPlatform.OpenAI => new OpenAITextEmbeddingGeneration(modelName, AiSetting.ApiKey, AiSetting.OrganizationId),

            //            AiPlatform.NeuCharOpenAI => new AzureTextEmbeddingGeneration(modelName, AiSetting.NeuCharEndpoint, AiSetting.ApiKey, AiSetting.NeuCharOpenAIApiVersion),

            //            AiPlatform.AzureOpenAI => new AzureTextEmbeddingGeneration(modelName, AiSetting.AzureEndpoint, AiSetting.ApiKey, AiSetting.AzureOpenAIApiVersion),

            //            _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            //        });
            //});

            //TODO:测试多次添加
            //KernelBuilder = builder;

            return kernelBuilder;
        }

        /// <summary>
        /// 设置 DallE 接口，默认强制使用 OpenAI 权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public KernelBuilder ConfigImageGeneration(string userId, KernelBuilder? kernelBuilder = null, string azureModeId = null)
        {
            var serviceId = GetServiceId(userId, "image-generation");
            var senparcAiSetting = Senparc.AI.Config.SenparcAiSetting;
            var aiPlatForm = AiSetting.AiPlatform;

            //TODO：Builder 不应该新建
            kernelBuilder ??= new KernelBuilder();

#pragma warning disable SKEXP0012
            _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextToImage(AiSetting.ApiKey,
                    AiSetting.OrganizationId),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextToImage(AiSetting.AzureEndpoint, azureModeId,
                    AiSetting.ApiKey),

                AiPlatform.NeuCharOpenAI => kernelBuilder.AddAzureOpenAITextToImage(
                    AiSetting.NeuCharEndpoint, azureModeId, AiSetting.ApiKey),

                _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            //kernelBuilder.Configure(c =>
            //{
            //    //c.AddImageGenerationService(serviceId, k =>
            //    //    aiPlatForm switch
            //    //    {
            //    //        AiPlatform.OpenAI => new OpenAIImageGeneration(AiSetting.ApiKey, AiSetting.OrganizationId),

            //    //        AiPlatform.AzureOpenAI => new OpenAIImageGeneration(AiSetting.ApiKey, AiSetting.OrganizationId),

            //    //        _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            //    //    });

            //    //强制使用 OpenAI 权限
            //    c.AddImageGenerationService(serviceId, k =>
            //        new OpenAIImageGeneration(AiSetting.OpenAIKeys.ApiKey, AiSetting.OpenAIKeys.OrganizationId));
            //});

            return kernelBuilder;
        }

        #region Memory 相关

        ISemanticTextMemory _textMemory = null;//TODO:适配多重不同的请求

        /// <summary>
        /// 获取 Kernel.Memory 对象
        /// </summary>
        /// <returns></returns>
        //[Obsolete("该方法已被SK放弃，原文为：Memory functionality will be placed in separate Microsoft.SemanticKernel.Plugins.Memory package. This will be removed in a future release. See sample dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs in the semantic-kernel repository.")]
#pragma warning disable SKEXP0001
        public ISemanticTextMemory? GetMemory(string modelName, ISenparcAiSetting senparcAiSetting,
            KernelBuilder? kernelBuilder, string azureDeployName = null, ITextEmbeddingGeneration textEmbeddingGeneration = null)
        {
            if (_textMemory == null)
            {
                senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
                var aiPlatForm = senparcAiSetting.AiPlatform;

                var memoryBuilder = new MemoryBuilder();

                _ = aiPlatForm switch
                {
                    AiPlatform.OpenAI => memoryBuilder.WithOpenAITextEmbeddingGeneration(modelName, senparcAiSetting.ApiKey, senparcAiSetting.OrganizationId),
                    AiPlatform.AzureOpenAI => memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(azureDeployName, modelName, senparcAiSetting.AzureEndpoint, senparcAiSetting.ApiKey),
                    AiPlatform.NeuCharOpenAI => memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(azureDeployName, modelName, senparcAiSetting.AzureEndpoint, senparcAiSetting.ApiKey),
                    AiPlatform.HuggingFace => memoryBuilder.WithTextEmbeddingGeneration(textEmbeddingGeneration),
                    _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
                };


                memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
                //.WithMemoryStore(new AzureAISearchMemoryStore(senparcAiSetting.AzureEndpoint, senparcAiSetting.ApiKey))

                _textMemory = memoryBuilder.Build();
            }


            return _textMemory;
        }



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

        #region RequestSettings

        /// <summary>
        /// 根据不同的 AiPlatform 类型生成不同的 ExecutionSettings 对象
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="topP"></param>
        /// <param name="maxTokens"></param>
        /// <param name="presencePenalty"></param>
        /// <param name="frequencyPenalty"></param>
        /// <param name="stopSequences"></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public PromptExecutionSettings GetExecutionSetting(double temperature = default, double topP = default, int? maxTokens = default, double presencePenalty = default, double frequencyPenalty = default, IList<string>? stopSequences = default, ISenparcAiSetting? senparcAiSetting = null)
        {
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
            var aiPlatForm = senparcAiSetting.AiPlatform;

            var promptExecutiongSetting = aiPlatForm switch
            {
                //AiPlatform.OpenAI => new OpenAIPromptExecutionSettings()
                //{
                //    Temperature = temperature,
                //    TopP = topP,
                //    MaxTokens = maxTokens,
                //    PresencePenalty = presencePenalty,
                //    FrequencyPenalty = frequencyPenalty,
                //    StopSequences = stopSequences
                //},
                //AiPlatform.AzureOpenAI =>
                //AiPlatform.NeuCharOpenAI => 
                //AiPlatform.HuggingFace => 
                _ => new OpenAIPromptExecutionSettings()
                {
                    Temperature = temperature,
                    TopP = topP,
                    MaxTokens = maxTokens,
                    PresencePenalty = presencePenalty,
                    FrequencyPenalty = frequencyPenalty,
                    StopSequences = stopSequences
                },
            };

            return promptExecutiongSetting;
        }

        /// <summary>
        /// 根据不同的 AiPlatform 类型生成不同的 ExecutionSettings 对象
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="topP"></param>
        /// <param name="maxTokens"></param>
        /// <param name="presencePenalty"></param>
        /// <param name="frequencyPenalty"></param>
        /// <param name="stopSequences"></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public PromptExecutionSettings GetExecutionSetting(PromptConfigParameter promptConfigParameter, ISenparcAiSetting? senparcAiSetting = null)
        {
            return GetExecutionSetting(
                   temperature: promptConfigParameter.Temperature ?? default,
                   topP: promptConfigParameter.TopP ?? default,
                   maxTokens: promptConfigParameter.MaxTokens,
                   presencePenalty: promptConfigParameter.PresencePenalty ?? default,
                   frequencyPenalty: promptConfigParameter.FrequencyPenalty ?? default,
                   stopSequences: promptConfigParameter.StopSequences,
                   senparcAiSetting
                   );
        }

        #endregion
    }
}