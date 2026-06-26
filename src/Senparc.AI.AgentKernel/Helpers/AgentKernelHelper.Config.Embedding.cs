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
        /// 设置 Kernel，并配置 TextCompletion 模型
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
            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            //TODO：Builder 不应该新建

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.TextEmbedding);

            string GetEndpointOrThrow(string? endpoint, string platformName)
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    throw new SenparcAiException($"{platformName} 必须提供 Endpoint");
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
                AiPlatform.Anthropic => throw new SenparcAiException("Anthropic 官方接口当前不提供 Embedding API，请切换到支持 Embedding 的平台。"),
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
                AiPlatform.XunFei => throw new SenparcAiException("XunFei 当前接入路径为 OpenAI-compatible Chat API，未包含 Embedding 接口。"),

                _ => throw new SenparcAiException($"ConfigTextEmbeddingGeneration 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            //TODO:测试多次添加
            //KernelBuilder = builder;
            return kernelBuilder;
        }

      
    }
}
