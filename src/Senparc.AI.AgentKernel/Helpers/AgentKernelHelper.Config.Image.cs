using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Helpers
{
    public partial class AgentKernelHelper
    {

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
        public IAIKernelBuilder ConfigImageGeneration(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null,
            IAIKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            senparcAiSetting ??= this.AiSetting;
            modelName ??= senparcAiSetting.ModelName.TextToImage;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;

            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.TextToImage);

            kernelBuilder.ImageClient = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextToImage(senparcAiSetting.ApiKey, modelName),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextToImage(
                    new Uri(senparcAiSetting.AzureEndpoint),
                    new ApiKeyCredential(senparcAiSetting.ApiKey),
                    new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName),
                _ => throw new SenparcAiException($"ConfigImageGeneration 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            return kernelBuilder;
        }

    }
}
