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

            var aiPlatForm = senparcAiSetting.AiPlatform;
            //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

            //TODO:Builder should not be recreated

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // The previous method has been marked obsolete by SK. Changed to the method recommended by SK.
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.TextEmbedding);

            string GetEndpointOrThrow(string? endpoint, string platformName)
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    throw new SenparcAiException($"{platformName} must provide Endpoint");
                }

                return endpoint;
            }

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
                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceEmbedding(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: senparcAiSetting.HuggingFaceEndpoint,
                    dimensions: senparcAiSetting.ModelName.EmbeddingDimensions),
                AiPlatform.FastAPI => kernelBuilder.AddFastAPIEmbedding(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.FastAPIEndpoint, nameof(AiPlatform.FastAPI)),
                    dimensions: senparcAiSetting.ModelName.EmbeddingDimensions),

                AiPlatform.Ollama => kernelBuilder.AddOllamaEmbedding(
                    endpoint: senparcAiSetting.OllamaEndpoint,
                    modelName: modelName),
                AiPlatform.DeepSeek => kernelBuilder.AddDeepSeekEmbedding(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.DeepSeekEndpoint, nameof(AiPlatform.DeepSeek))),
                AiPlatform.Anthropic => throw new SenparcAiException("The official Anthropic API does not currently provide an Embedding API. Switch to a platform that supports Embedding."),
                AiPlatform.Gemini => kernelBuilder.AddOpenAICompatibleEmbedding(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.GeminiEndpoint, nameof(AiPlatform.Gemini))),
                AiPlatform.Qwen => kernelBuilder.AddOpenAICompatibleEmbedding(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.QwenEndpoint, nameof(AiPlatform.Qwen))),
                AiPlatform.Kimi => kernelBuilder.AddOpenAICompatibleEmbedding(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.KimiEndpoint, nameof(AiPlatform.Kimi))),
                AiPlatform.XunFei => throw new SenparcAiException("The current XunFei integration path is the OpenAI-compatible Chat API and does not include an Embedding API."),

                _ => throw new SenparcAiException($"ConfigTextEmbeddingGeneration does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            //TODO:Test repeated additions
            //KernelBuilder = builder;
            return kernelBuilder;
        }

      
    }
}
