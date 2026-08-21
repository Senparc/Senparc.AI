using Azure.AI.OpenAI;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.ClientModel;

namespace Senparc.AI.AgentKernel.Helpers
{
    public partial class AgentKernelHelper
    {
        /// <summary>
        /// Configures the speech-to-text model (OpenAI Whisper).
        /// </summary>
        /// <param name="userId">User ID used to distinguish service instances.</param>
        /// <param name="kernelBuilder">Existing builder, allowing multiple models to be configured in sequence.</param>
        /// <param name="modelName">Model name. If empty, the value is read from configuration and defaults to whisper.</param>
        /// <param name="senparcAiSetting">AI configuration.</param>
        /// <param name="deploymentName">Azure deployment name. If empty, DeploymentName or modelName is used.</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public IAIKernelBuilder ConfigSpeechToText(
            string userId,
            string modelName = null,
            ISenparcAiSetting senparcAiSetting = null,
            IAIKernelBuilder? kernelBuilder = null,
            string deploymentName = null)
        {
            senparcAiSetting ??= this.AiSetting;
            modelName ??= senparcAiSetting.ModelName.SpeechToText ?? "whisper";
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var aiPlatform = senparcAiSetting.AiPlatform;
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.SpeechToText);

            kernelBuilder.SpeechToTextClient = aiPlatform switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIAudio(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIAudio(
                    endpoint: new Uri(senparcAiSetting.AzureEndpoint),
                    credential: new ApiKeyCredential(senparcAiSetting.ApiKey),
                    options: new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName),

                AiPlatform.NeuCharAI => kernelBuilder.AddNeuCharAIAudio(
                    endpoint: new Uri(senparcAiSetting.NeuCharEndpoint),
                    credential: new ApiKeyCredential(senparcAiSetting.ApiKey),
                    options: new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName),

                _ => throw new SenparcAiException($"ConfigSpeechToText does not handle the current {nameof(AiPlatform)} type: {aiPlatform}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Preserves the legacy name for configuring speech-to-text.
        /// </summary>
        public IAIKernelBuilder ConfigAudioToText(
            string userId,
            IAIKernelBuilder? kernelBuilder = null,
            string modelName = null,
            ISenparcAiSetting senparcAiSetting = null,
            string deploymentName = null)
        {
            return ConfigSpeechToText(userId, modelName, senparcAiSetting, kernelBuilder, deploymentName);
        }

        /// <summary>
        /// Configures the text-to-speech model.
        /// </summary>
        /// <param name="userId">User ID used to distinguish service instances.</param>
        /// <param name="modelName">Model name. If empty, the value is read from configuration and defaults to tts.</param>
        /// <param name="senparcAiSetting">AI configuration.</param>
        /// <param name="kernelBuilder">Existing builder, allowing multiple models to be configured in sequence.</param>
        /// <param name="deploymentName">Azure deployment name. If empty, DeploymentName or modelName is used.</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public IAIKernelBuilder ConfigTextToSpeech(
            string userId,
            string modelName = null,
            ISenparcAiSetting senparcAiSetting = null,
            IAIKernelBuilder? kernelBuilder = null,
            string deploymentName = null)
        {
            senparcAiSetting ??= this.AiSetting;
            modelName ??= senparcAiSetting.ModelName.TextToSpeech ?? "tts";
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var aiPlatform = senparcAiSetting.AiPlatform;
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.TextToSpeech);

            kernelBuilder.TextToSpeechClient = aiPlatform switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIAudio(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIAudio(
                    endpoint: new Uri(senparcAiSetting.AzureEndpoint),
                    credential: new ApiKeyCredential(senparcAiSetting.ApiKey),
                    options: new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName),

                AiPlatform.NeuCharAI => kernelBuilder.AddNeuCharAIAudio(
                    endpoint: new Uri(senparcAiSetting.NeuCharEndpoint),
                    credential: new ApiKeyCredential(senparcAiSetting.ApiKey),
                    options: new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName),

                _ => throw new SenparcAiException($"ConfigTextToSpeech does not handle the current {nameof(AiPlatform)} type: {aiPlatform}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// Preserves the legacy name for configuring text-to-speech.
        /// </summary>
        public IAIKernelBuilder ConfigTextToAudio(
            string userId,
            IAIKernelBuilder? kernelBuilder = null,
            string modelName = null,
            ISenparcAiSetting senparcAiSetting = null,
            string deploymentName = null)
        {
            return ConfigTextToSpeech(userId, modelName, senparcAiSetting, kernelBuilder, deploymentName);
        }
    }
}
