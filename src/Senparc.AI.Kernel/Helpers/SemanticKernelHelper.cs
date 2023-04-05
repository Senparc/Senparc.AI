using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.OpenAI.HttpSchema;
using Microsoft.SemanticKernel.AI.OpenAI.Services;
using Microsoft.SemanticKernel.Configuration;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Polly;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
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

            //TODO:AddTextCompletion也要独立出来

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

}
