using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.OpenAI.HttpSchema;
using Microsoft.SemanticKernel.AI.OpenAI.Services;
using Microsoft.SemanticKernel.Configuration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.CO2NET;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Senparc.AI.Kernel.Helpers
{
    /// <summary>
    /// SemanticKernel 帮助类
    /// </summary>
    public class SemanticKernelHelper
    {
        internal IKernel Kernel { get; set; }
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
        /// 设置 Kernel
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IKernel Config(string userId, string modelName, IKernel? kernel = null)
        {
            kernel ??= GetKernel();

            var serviceId = GetServiceId(userId, modelName);
            var senparcAiSetting = Senparc.AI.Config.SenparcAiSetting;
            switch (senparcAiSetting.AiPlatform)
            {
                case AiPlatform.OpenAI:
                    kernel.Config.AddAzureOpenAITextCompletion(serviceId, modelName, senparcAiSetting.AzureEndpoint, senparcAiSetting.ApiKey);
                    break;
                case AiPlatform.AzureOpenAI:
                    kernel.Config.AddOpenAITextCompletion(serviceId, modelName, senparcAiSetting.ApiKey, senparcAiSetting.OrgaizationId);
                    break;
                default:
                    throw new Senparc.AI.Exceptions.SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{senparcAiSetting.AiPlatform}");
            }

            return kernel;
        }
    }

    public class IWantTo
    {
        public IKernel Kernel { get; set; }
        public KernelConfig KernelConfig { get; set; }
        public SemanticKernelHelper SemanticKernelHelper { get; set; }

        public string UserId { get; set; }
        public string ModelName { get; set; }

        public IWantTo() { }

        public IWantTo(KernelConfig kernelConfig)
        {
            KernelConfig = kernelConfig;
        }

        public IWantTo(SemanticKernelHelper semanticKernelHelper)
        {
            SemanticKernelHelper = semanticKernelHelper;
        }


        public IWantTo Config(string userId, string modelName)
        {
            UserId = userId;
            ModelName = modelName;
            return this;
        }
    }

    public class IWantToConfig
    {
        public IWantTo IWantTo { get; set; }

        public IWantToConfig(IWantTo iWantTo)
        {
            IWantTo = iWantTo;
        }
    }

    public static class KernelConfigExtension
    {
        public static IWantTo IWantTo(this SemanticKernelHelper sKHelper)
        {
            var iWantTo = new IWantTo(sKHelper);
            return iWantTo;
        }

        public static IWantToConfig Config(this IWantTo iWantTo, string userId, string modelName)
        {
            var kernel = iWantTo.SemanticKernelHelper.Config(userId, modelName);
            iWantTo.Kernel = kernel;//进行 Config 必须提供 Kernel
            return new IWantToConfig(iWantTo);
        }

        /// <summary>
        /// 添加 TextCompletion 配置
        /// </summary>
        /// <param name="iWantToConfig"></param>
        /// <param name="modelName">如果为 null，则从先前配置中读取</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public static IWantToConfig AddTextCompletion(this IWantToConfig iWantToConfig, string? modelName = null)
        {
            var aiPlatForm = iWantToConfig.IWantTo.SemanticKernelHelper.AiSetting.AiPlatform;
            var kernel = iWantToConfig.IWantTo.Kernel;
            var skHelper = iWantToConfig.IWantTo.SemanticKernelHelper;
            var aiSetting = skHelper.AiSetting;
            var userId = iWantToConfig.IWantTo.UserId;
            modelName ??= iWantToConfig.IWantTo.ModelName;
            var serviceId = skHelper.GetServiceId(userId, modelName);

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            kernel.Config.AddTextCompletion(serviceId, k =>
                aiPlatForm switch
                {
                    AiPlatform.OpenAI => new OpenAITextCompletion(modelName, aiSetting.ApiKey, aiSetting.OrgaizationId),

                    AiPlatform.AzureOpenAI => new AzureTextCompletion(modelName, aiSetting.AzureEndpoint, aiSetting.ApiKey, aiSetting.AzureOpenAIApiVersion),

                    _ =>
                        throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
                }
            );

            return iWantToConfig;
        }

        public static IWantTo RegisterSemanticFunction(this SemanticKernelHelper helper, PromptConfigParameter  promptConfigPara, string? modelName = null)
        {
            var promptConfig = new PromptTemplateConfig
            {
                Completion =
                    {
                        MaxTokens = 2000,
                        Temperature = 0.7,
                        TopP = 0.5,
                    }
            };

            return new IWantTo(helper);
        }
    }
}
