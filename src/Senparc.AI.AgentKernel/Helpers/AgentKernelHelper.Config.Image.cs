using Azure.AI.OpenAI;
using OpenAI;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Helpers
{
    public partial class AgentKernelHelper
    {

        /// <summary>
        /// Set the DallE interface. OpenAI credentials are forced by default.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kernelBuilder"></param>
        /// <param name="azureModeId">AzureOpenAI model name</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="azureDallEDepploymentName">AzureAI DallE model deployment name</param>
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
                AiPlatform.OpenAI => kernelBuilder.AddOpenAITextToImage(senparcAiSetting.ApiKey, modelName, new OpenAIClientOptions()),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextToImage(senparcAiSetting, senparcAiSetting.ApiKey, modelName, new OpenAIClientOptions()),
                AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAITextToImage(senparcAiSetting, senparcAiSetting.ApiKey, modelName, new OpenAIClientOptions()),
                //new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                _ => throw new SenparcAiException($"ConfigImageGeneration does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            return kernelBuilder;
        }

    }
}
