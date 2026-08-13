using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using Senparc.AI.AgentKernel.Providers.FastAPI;
using Senparc.AI.AgentKernel.Providers.HuggingFace;
using System.ClientModel;

namespace Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions
{
    public static partial class KernelBuilderExtension
    {
        public static ChatClient AddOpenAIChatCompletion(this IAIKernelBuilder kernelBuilder,
            string apiKey, string modelName, OpenAIClientOptions? options = null)
        {
            return new OpenAIClient(
                new ApiKeyCredential(apiKey ?? string.Empty),
                options ?? new OpenAIClientOptions()).GetChatClient(modelName);
        }

        public static ChatClient AddOpenAICompatibleChatCompletion(this IAIKernelBuilder kernelBuilder,
            string apiKey, string modelName, string endpoint, OpenAIClientOptions? options = null)
        {
            options ??= new OpenAIClientOptions();
            options.Endpoint = new Uri(endpoint);
            var client = new OpenAIClient(
                credential: new ApiKeyCredential(apiKey ?? string.Empty),
                options: options);

            return client.GetChatClient(modelName);
        }

        public static ChatClient AddAzureOpenAIChatCompletion(this IAIKernelBuilder kernelBuilder,
            Uri endpoint,
            ApiKeyCredential credential,
            AzureOpenAIClientOptions options,
            string deploymentName)
        {
            return new AzureOpenAIClient(endpoint, credential, options).GetChatClient(deploymentName);
        }

        public static ChatClient AddNeuCharAIChatCompletion(this IAIKernelBuilder kernelBuilder,
            Uri endpoint,
            ApiKeyCredential credential,
            AzureOpenAIClientOptions options,
            string deploymentName)
        {
            return kernelBuilder.AddAzureOpenAIChatCompletion(endpoint, credential, options, deploymentName);
        }

        public static IChatClient AddOllamaChatCompletion(this IAIKernelBuilder kernelBuilder, string endpoint, string modelName)
        {
            return new OllamaApiClient(endpoint, modelName);
        }

        public static IChatClient AddFastAPIChatCompletion(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName,
            string endpoint)
        {
            return FastAPIProviderClientFactory.CreateChatClient(
                modelName: modelName,
                endpoint: endpoint,
                apiKey: apiKey);
        }

        public static IChatClient AddHuggingFaceChatCompletion(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName,
            string? endpoint)
        {
            return HuggingFaceProviderClientFactory.CreateChatClient(
                modelName: modelName,
                endpoint: endpoint,
                apiKey: apiKey);
        }

        public static ChatClient AddDeepSeekChatCompletion(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName,
            string endpoint,
            OpenAIClientOptions? options = null)
        {
            return kernelBuilder.AddOpenAICompatibleChatCompletion(apiKey, modelName, endpoint, options);
        }

        public static ChatClient AddXunFeiChatCompletion(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName,
            string endpoint,
            OpenAIClientOptions? options = null)
        {
            return kernelBuilder.AddOpenAICompatibleChatCompletion(apiKey, modelName, endpoint, options);
        }
    }
}
