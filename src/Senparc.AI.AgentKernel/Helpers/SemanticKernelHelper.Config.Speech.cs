using Azure.AI.OpenAI;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Helpers
{
    public partial class SemanticKernelHelper
    {


        /// <summary>
        /// 设置 Whisper 语音转文字接口
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kernelBuilder"></param>
        /// <param name="modelId">模型名称，默认为 whisper</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="deploymentName">Azure 部署名称</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        //public IAIKernelBuilder ConfigAudioToText(string userId, IAIKernelBuilder? kernelBuilder = null, string modelId = null, ISenparcAiSetting senparcAiSetting = null, string deploymentName = null)
        //{
        //    senparcAiSetting ??= this.AiSetting;
        //    modelId ??= "whisper"; // 默认使用 whisper 模型

        //    var serviceId = GetServiceId(userId, "audio-to-text");
        //    var aiPlatForm = senparcAiSetting.AiPlatform;

        //    kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

        //    _ = aiPlatForm switch
        //    {
        //        AiPlatform.OpenAI => kernelBuilder.AddOpenAIAudioToText(
        //            modelId: modelId,
        //            apiKey: senparcAiSetting.ApiKey,
        //            orgId: senparcAiSetting.OrganizationId,
        //            httpClient: _httpClient),

        //        AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIAudioToText(
        //            deploymentName: deploymentName ?? modelId,
        //            endpoint: senparcAiSetting.Endpoint,
        //            apiKey: senparcAiSetting.ApiKey,
        //            httpClient: _httpClient),

        //        AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAIAudioToText(
        //            deploymentName: deploymentName ?? modelId,
        //            endpoint: senparcAiSetting.Endpoint,
        //            apiKey: senparcAiSetting.ApiKey,
        //            httpClient: _httpClient),

        //        _ => throw new SenparcAiException($"ConfigAudioToText 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
        //    };

        //    return kernelBuilder;
        //}
    }
}
