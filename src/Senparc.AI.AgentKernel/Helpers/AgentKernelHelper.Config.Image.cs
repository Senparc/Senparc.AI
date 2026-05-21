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
        /// 设置 DallE 接口，默认强制使用 OpenAI 权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="kernelBuilder"></param>
        /// <param name="azureModeId">AzureOpenAI 的模型名称</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="azureDallEDepploymentName">AzureAI 的 DallE 模型部署名称</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        //public IAIKernelBuilder ConfigImageGeneration(string userId, IAIKernelBuilder? kernelBuilder = null, string azureModeId = null, ISenparcAiSetting senparcAiSetting = null, string azureDallEDepploymentName = null)
        //{
        //    senparcAiSetting ??= this.AiSetting;

        //    var serviceId = GetServiceId(userId, "image-generation");
        //    var aiPlatForm = senparcAiSetting.AiPlatform;

        //    //TODO：Builder 不应该新建
        //    kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

        //    _ = aiPlatForm switch
        //    {
        //        AiPlatform.OpenAI => kernelBuilder.AddOpenAITextToImage(
        //            apiKey: senparcAiSetting.ApiKey,
        //            orgId: senparcAiSetting.OrganizationId,
        //            httpClient: _httpClient),

        //        AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAITextToImage(
        //            deploymentName: azureDallEDepploymentName,
        //            endpoint: senparcAiSetting.Endpoint,
        //            apiKey: senparcAiSetting.ApiKey,
        //            modelId: azureModeId,
        //            httpClient: _httpClient),

        //        AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAITextToImage(
        //            deploymentName: azureDallEDepploymentName,
        //            endpoint: senparcAiSetting.Endpoint,
        //            apiKey: senparcAiSetting.ApiKey,
        //            modelId: azureModeId,
        //            httpClient: _httpClient),

        //        _ => throw new SenparcAiException($"ConfigImageGeneration 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
        //    };

        //    return kernelBuilder;
        //}

    }
}
