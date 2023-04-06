using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Polly;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;
using Senparc.CO2NET;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Helpers
{
    /// <summary>
    /// SemanticKernel 帮助类
    /// </summary>
    public class SemanticKernelHelper
    {
        internal IKernel Kernel { get; set; }
        //internal KernelBuilder KernelBuilder { get; set; }
        internal ISenparcAiSetting AiSetting { get; }

        public SemanticKernelHelper(ISenparcAiSetting aiSetting = null)
        {
            AiSetting = aiSetting ?? Senparc.AI.Config.SenparcAiSetting;
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
        /// <param name="refresh"></param>
        /// <returns></returns>
        public IKernel GetKernel(Action<KernelBuilder>? kernelBuilderAction = null, bool refresh = false)
        {
            if (Kernel == null || refresh == true)
            {
                //Kernel = KernelBuilder.Create();
                var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
                if (kernelBuilderAction != null)
                {
                    kernelBuilderAction.Invoke(kernelBuilder);
                }
                Kernel = kernelBuilder.Build();
            }
            return Kernel;
        }

        /// <summary>
        /// Build 新的 Kernel 对象
        /// </summary>
        /// <param name="kernelBuilder"></param>
        /// <param name="kernelBuilderAction"></param>
        /// <returns></returns>
        public IKernel BuildKernel(KernelBuilder kernelBuilder, Action<KernelBuilder>? kernelBuilderAction = null)
        {
            if (kernelBuilderAction != null)
            {
                kernelBuilderAction.Invoke(kernelBuilder);
            }
            Kernel = kernelBuilder.Build();
            return Kernel;
        }

        /// <summary>
        /// 设置 Kernel，并配置 TextCompletion 模型
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public KernelBuilder ConfigTextCompletion(string userId, string modelName, KernelBuilder kernelBuilder = null)
        {
            //kernel ??= GetKernel();

            var serviceId = GetServiceId(userId, modelName);
            var senparcAiSetting = Senparc.AI.Config.SenparcAiSetting;
            var aiPlatForm = AiSetting.AiPlatform;

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.Builder;

            kernelBuilder.Configure(c =>
            {
                c.AddTextCompletionService(serviceId, k =>
                    aiPlatForm switch
                    {
                        AiPlatform.OpenAI => new OpenAITextCompletion(modelName, AiSetting.ApiKey, AiSetting.OrgaizationId),

                        AiPlatform.AzureOpenAI => new AzureTextCompletion(modelName, AiSetting.AzureEndpoint, AiSetting.ApiKey, AiSetting.AzureOpenAIApiVersion),

                        _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
                    });
            });

            //KernelBuilder = builder;

            return kernelBuilder;
        }

        /// <summary>
        /// 设置 Kernel，并配置 TextCompletion 模型
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public KernelBuilder ConfigTextEmbeddingGeneration(string userId, string modelName, KernelBuilder? kernelBuilder=null)
        {
            //kernel ??= GetKernel();

            var serviceId = GetServiceId(userId, modelName);
            var senparcAiSetting = Senparc.AI.Config.SenparcAiSetting;
            var aiPlatForm = AiSetting.AiPlatform;

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            //TODO：Builder 不应该新建
            kernelBuilder ??= Microsoft.SemanticKernel.Kernel.Builder;

            kernelBuilder.Configure(c =>
            {
                c.AddTextEmbeddingGenerationService(serviceId, k =>
                    aiPlatForm switch
                    {
                        AiPlatform.OpenAI => new OpenAITextEmbeddingGeneration(modelName, AiSetting.ApiKey, AiSetting.OrgaizationId),

                        AiPlatform.AzureOpenAI => new AzureTextEmbeddingGeneration(modelName, AiSetting.AzureEndpoint, AiSetting.ApiKey, AiSetting.AzureOpenAIApiVersion),

                        _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
                    });
            });

            //TODO:测试多次添加
            //KernelBuilder = builder;

            return kernelBuilder;
        }
    }

}
