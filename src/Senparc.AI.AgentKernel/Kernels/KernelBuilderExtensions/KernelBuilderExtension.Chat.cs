using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions
{
    public static partial class KernelBuilderExtension
    {


        public static ChatClient AddOpenAIChatCompletion(this IAIKernelBuilder kernelBuilder,
            string apiKey, string modelName)
        {
            return new OpenAIClient(apiKey).GetChatClient(modelName);
        }

        public static ChatClient AddAzureOpenAIChatCompletion(this IAIKernelBuilder kernelBuilder, Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string deploymentName)
        {
            return new AzureOpenAIClient(endpoint, credential, options).GetChatClient(deploymentName);
        }

        public static ChatClient AddNeuCharAIChatCompletion(this IAIKernelBuilder kernelBuilder, Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string deploymentName)
        {
            return kernelBuilder.AddAzureOpenAIChatCompletion(endpoint, credential, options, deploymentName);
        }


        public static IChatClient AddOllamaChatCompletion(this IAIKernelBuilder kernelBuilder, string endpoint, string modelName)
        {
            return new OllamaApiClient(endpoint, modelName);
        }

        public static ChatClient AddHuggingFaceChatCompletion(this IAIKernelBuilder kernelBuilder, Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string deploymentName)
        {
            return kernelBuilder.AddAzureOpenAIChatCompletion(endpoint, credential, options, deploymentName);
        }

        public static ChatClient AddDeepSeekChatCompletion(this IAIKernelBuilder kernelBuilder,
            string apiKey, string modelName)
        {
            return kernelBuilder.AddOpenAIChatCompletion(apiKey, modelName);
        }

    }
}
