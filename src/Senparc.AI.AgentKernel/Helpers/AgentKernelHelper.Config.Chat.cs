using Azure.AI.OpenAI;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Text;
using OpenAI;

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

            var aiPlatForm = senparcAiSetting.AiPlatform;

            //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // The previous method has been marked obsolete by SK. Changed to the method recommended by SK.
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.Chat);

            string GetEndpointOrThrow(string? endpoint, string platformName)
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    throw new SenparcAiException($"{platformName} must provide Endpoint");
                }

                return endpoint;
            }

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            kernelBuilder.ChatClient = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(
                    senparcAiSetting.ApiKey,
                    modelName,
                    CreateOpenAIClientOptions()),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                    new Uri(senparcAiSetting.AzureEndpoint),
                    new ApiKeyCredential(senparcAiSetting.ApiKey),
                    CreateAzureOpenAIClientOptions(),
                    deploymentName: deploymentName
                ),
                AiPlatform.NeuCharAI => kernelBuilder.AddNeuCharAIChatCompletion(
                    new Uri(senparcAiSetting.NeuCharEndpoint),
                    new ApiKeyCredential(senparcAiSetting.ApiKey),
                    CreateAzureOpenAIClientOptions(),
                    deploymentName: deploymentName
                ),
                AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceChatCompletion(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: senparcAiSetting.HuggingFaceEndpoint),
                AiPlatform.FastAPI => kernelBuilder.AddFastAPIChatCompletion(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.FastAPIEndpoint, nameof(AiPlatform.FastAPI))),
                AiPlatform.Ollama => kernelBuilder.AddOllamaChatCompletion(senparcAiSetting.OllamaEndpoint, modelName),
                // These platforms use the OpenAI-compatible Chat API protocol, or a compatible gateway.
                AiPlatform.DeepSeek => kernelBuilder.AddDeepSeekChatCompletion(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.DeepSeekEndpoint, nameof(AiPlatform.DeepSeek)),
                    options: CreateOpenAIClientOptions()),
                AiPlatform.Anthropic => kernelBuilder.AddOpenAICompatibleChatCompletion(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.AnthropicEndpoint, nameof(AiPlatform.Anthropic)),
                    options: CreateOpenAIClientOptions()),
                AiPlatform.Gemini => kernelBuilder.AddOpenAICompatibleChatCompletion(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.GeminiEndpoint, nameof(AiPlatform.Gemini)),
                    options: CreateOpenAIClientOptions()),
                AiPlatform.Qwen => kernelBuilder.AddOpenAICompatibleChatCompletion(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.QwenEndpoint, nameof(AiPlatform.Qwen)),
                    options: CreateOpenAIClientOptions()),
                AiPlatform.Kimi => kernelBuilder.AddOpenAICompatibleChatCompletion(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.KimiEndpoint, nameof(AiPlatform.Kimi)),
                    options: CreateOpenAIClientOptions()),
                AiPlatform.XunFei => kernelBuilder.AddXunFeiChatCompletion(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName,
                    endpoint: GetEndpointOrThrow(senparcAiSetting.XunFeiEndpoint, nameof(AiPlatform.XunFei)),
                    options: CreateOpenAIClientOptions()),

                _ => throw new SenparcAiException($"ConfigChat does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Creates provider options using the <see cref="HttpClient"/> configured on this helper.
        /// Without an explicit transport the SDK creates an unrelated client pipeline and bypasses
        /// caller-supplied handlers (proxy, diagnostics, retry policy and test transport).
        /// </summary>
        private AzureOpenAIClientOptions CreateAzureOpenAIClientOptions()
        {
            var options = new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview);
            ConfigurePipelineTransport(options);
            return options;
        }

        private OpenAIClientOptions CreateOpenAIClientOptions()
        {
            var options = new OpenAIClientOptions();
            ConfigurePipelineTransport(options);
            return options;
        }

        private void ConfigurePipelineTransport(ClientPipelineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // The helper owns neither a caller-supplied HttpClient nor its lifetime. Individual SDK
            // clients are short-lived during Agent construction, so they must not dispose this client.
            options.Transport = new HttpClientPipelineTransport(_httpClient, false, loggerFactory);
        }
    }
}
