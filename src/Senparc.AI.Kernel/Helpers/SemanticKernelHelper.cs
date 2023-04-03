using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Senparc.AI.Helpers
{
    /// <summary>
    /// SemanticKernel 帮助类
    /// </summary>
    public class SemanticKernelHelper
    {
        public SemanticKernelHelper() { }

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
        /// <returns></returns>
        public IKernel GetKernel()
        {
            IKernel kernel = Microsoft.SemanticKernel.Kernel.Builder.Build();
            return kernel;
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
}
