using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Images;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using Senparc.AI.Interfaces;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions
{
    public static partial class KernelBuilderExtension
    {
        #region Image


        //public static ImageClientDescriptor AddOpenAITextToImage(this IAIKernelBuilder kernelBuilder, string apiKey, string modelName)
        //{
        //    return new ImageClientDescriptor { Provider = "OpenAI", ApiKey = apiKey, Model = modelName };
        //}

        //public static ImageClientDescriptor AddAzureOpenAITextToImage(this IAIKernelBuilder kernelBuilder, Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string deploymentName)
        //{
        //    return new ImageClientDescriptor { Provider = "AzureOpenAI", Endpoint = endpoint, Credential = credential, Options = options, Deployment = deploymentName };
        //}

        public static ImageClient AddOpenAITextToImage(this IAIKernelBuilder kernelBuilder, string apiKey, string modelName, OpenAIClientOptions options /*AzureOpenAIClientOptions options*/)
        {
            return new ImageClient(modelName, new ApiKeyCredential(apiKey), options);
        }

        public static ImageClient AddAzureOpenAITextToImage(this IAIKernelBuilder kernelBuilder, ISenparcAiSetting senparcAiSetting, string apiKey, string modelName, OpenAIClientOptions options /*AzureOpenAIClientOptions options*/)
        {
            AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(senparcAiSetting.Endpoint), new ApiKeyCredential(apiKey));

            ImageClient client = azureClient.GetImageClient(modelName);

            return client;
            //return new ImageClient(modelName, new ApiKeyCredential(apiKey), new OpenAIClientOptions() { Endpoint = new Uri(senparcAiSetting.Endpoint) });
        }


        #endregion
    }
}
