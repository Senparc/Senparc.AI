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
        /// Set Kernel and configure the TextCompletion model
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IAIKernelBuilder ConfigTextEmbeddingGeneration(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null, IAIKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            //kernel ??= GetKernel();

            senparcAiSetting ??= this.AiSetting;
            modelName ??= senparcAiSetting.ModelName.Embedding;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;
            //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

            //TODO:Builder should not be recreated

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // The previous method has been marked obsolete by SK. Changed to the method recommended by SK.
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.TextEmbedding);

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            kernelBuilder.EmbeddingClient = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIEmbedding(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIEmbedding(
                    endpoint: new Uri(senparcAiSetting.AzureEndpoint),
                    credential: new System.ClientModel.ApiKeyCredential(senparcAiSetting.ApiKey),
                    options: new Azure.AI.OpenAI.AzureOpenAIClientOptions(),
                    azureDeploymentName : deploymentName),
                AiPlatform.NeuCharAI => kernelBuilder.AddNeuCharAIEmbedding(
                    endpoint: new Uri(senparcAiSetting.NeuCharEndpoint),
                    credential: new System.ClientModel.ApiKeyCredential(senparcAiSetting.ApiKey),
                    options: new Azure.AI.OpenAI.AzureOpenAIClientOptions(),
                    modelName: deploymentName),
                //AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextEmbeddingGeneration(
                //    model: modelName,
                //    endpoint: new Uri(senparcAiSetting.Endpoint ?? throw new SenparcAiException("HuggingFace requires Endpoint")),
                //    httpClient: _httpClient),

                AiPlatform.Ollama => kernelBuilder.AddOllamaEmbedding(
                    endpoint: senparcAiSetting.OllamaEndpoint,
                    modelName: modelName),

                _ => throw new SenparcAiException($"ConfigTextEmbeddingGeneration does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            //TODO:Test repeated additions
            //KernelBuilder = builder;
            return kernelBuilder;
        }

      
    }
}
