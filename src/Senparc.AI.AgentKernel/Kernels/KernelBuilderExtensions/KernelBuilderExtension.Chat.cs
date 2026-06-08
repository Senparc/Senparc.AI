using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions
{
    public static partial class KernelBuilderExtension
    {
        #region Image

        public static object AddOpenAITextToImage(this IAIKernelBuilder kernelBuilder, string apiKey, string modelName)
        {
            // Use Azure/OpenAI or community OpenAI SDKs if available. For now store info as a simple anonymous holder
            return new { Provider = "OpenAI", ApiKey = apiKey, Model = modelName };
        }

        public static object AddAzureOpenAITextToImage(this IAIKernelBuilder kernelBuilder, Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string deploymentName)
        {
            return new { Provider = "AzureOpenAI", Endpoint = endpoint, Credential = credential, Options = options, Deployment = deploymentName };
        }

        #endregion

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
