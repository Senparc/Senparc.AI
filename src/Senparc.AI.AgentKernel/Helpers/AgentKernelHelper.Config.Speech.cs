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
        /// 配置语音转文字（Speech-To-Text）模型（OpenAI Whisper）。
        /// </summary>
        /// <param name="userId">用户 ID，用于区分服务实例。</param>
        /// <param name="kernelBuilder">已有 builder，可连续配置多个模型。</param>
        /// <param name="modelName">模型名称（为空则读取配置，默认 whisper）。</param>
        /// <param name="senparcAiSetting">AI 配置。</param>
        /// <param name="deploymentName">Azure 部署名（为空则取 DeploymentName 或 modelName）。</param>
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

                _ => throw new SenparcAiException($"ConfigSpeechToText 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatform}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// 保留旧命名：配置语音转文字（Speech-To-Text）。
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
        /// 配置文本转语音（Text-To-Speech）模型。
        /// </summary>
        /// <param name="userId">用户 ID，用于区分服务实例。</param>
        /// <param name="modelName">模型名称（为空则读取配置，默认 tts）。</param>
        /// <param name="senparcAiSetting">AI 配置。</param>
        /// <param name="kernelBuilder">已有 builder，可连续配置多个模型。</param>
        /// <param name="deploymentName">Azure 部署名（为空则取 DeploymentName 或 modelName）。</param>
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

                _ => throw new SenparcAiException($"ConfigTextToSpeech 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatform}")
            };

            return kernelBuilder;
        }

        /// <summary>
        /// 保留旧命名：配置文本转语音（Text-To-Speech）。
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
