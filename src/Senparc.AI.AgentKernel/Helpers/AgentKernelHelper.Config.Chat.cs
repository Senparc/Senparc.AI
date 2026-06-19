using Azure.AI.OpenAI;
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
        /// Set Kernel and configure the TextCompletion model
        /// </summary>
        /// <param name="userId">User ID, used to prevent API abuse</param>
        /// <param name="modelName">Model name modelId</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IAIKernelBuilder ConfigChat(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null,
            IAIKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
            modelName ??= senparcAiSetting.ModelName.Chat;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;

            //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // The previous method has been marked obsolete by SK. Changed to the method recommended by SK.
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.Chat);

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            kernelBuilder.ChatClient = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(senparcAiSetting.ApiKey, modelName),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                    new Uri(senparcAiSetting.AzureEndpoint),
                    new ApiKeyCredential(senparcAiSetting.ApiKey),
                    new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName
                ),
                AiPlatform.NeuCharAI => kernelBuilder.AddNeuCharAIChatCompletion(
                    new Uri(senparcAiSetting.NeuCharEndpoint),
                    new ApiKeyCredential(senparcAiSetting.ApiKey),
                    new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName
                ),
                //AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextGeneration(
                //        model: modelName,
                //        apiKey: null,
                //        endpoint: new Uri(senparcAiSetting.HuggingFaceEndpoint ?? throw new SenparcAiException("HuggingFace requires Endpoint")),
                //        serviceId: serviceId,
                //        httpClient: _httpClient),
                //AiPlatform.FastAPI => kernelBuilder.AddFastAPIChatCompletion(
                //        modelId: modelName,
                //        apiKey: senparcAiSetting.ApiKey,
                //        orgId: senparcAiSetting.OrganizationId,
                //        endpoint: senparcAiSetting.FastAPIEndpoint,
                //        serviceId: null
                //    ),
                AiPlatform.Ollama => kernelBuilder.AddOllamaChatCompletion(senparcAiSetting.OllamaEndpoint, modelName),
                //DeepSeek uses the same request format as OpenAI
                AiPlatform.DeepSeek => kernelBuilder.AddOpenAIChatCompletion(senparcAiSetting.ApiKey, modelName),

                _ => throw new SenparcAiException($"ConfigChat does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            return kernelBuilder;
        }
    }
}
