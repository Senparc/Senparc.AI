using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Audio;
using System;
using System.ClientModel;

namespace Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions
{
    public static partial class KernelBuilderExtension
    {
        #region Speech

        public static AudioClient AddOpenAIAudio(this IAIKernelBuilder kernelBuilder, string apiKey, string modelName)
        {
            return new OpenAIClient(apiKey).GetAudioClient(modelName);
        }

        public static AudioClient AddAzureOpenAIAudio(this IAIKernelBuilder kernelBuilder, Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string deploymentName)
        {
            return new AzureOpenAIClient(endpoint, credential, options).GetAudioClient(deploymentName);
        }

        public static AudioClient AddNeuCharAIAudio(this IAIKernelBuilder kernelBuilder, Uri endpoint, ApiKeyCredential credential, AzureOpenAIClientOptions options, string deploymentName)
        {
            return kernelBuilder.AddAzureOpenAIAudio(endpoint, credential, options, deploymentName);
        }

        #endregion
    }
}
