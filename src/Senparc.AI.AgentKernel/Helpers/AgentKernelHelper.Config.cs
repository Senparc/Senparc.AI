using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SemanticKernel;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System.Threading.Tasks;
using System.Threading;
using Senparc.CO2NET.Extensions;
using Microsoft.Extensions.Http.Logging;
using System.Net.Http;
//using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System.Net;
using System.Runtime.CompilerServices;
using Senparc.AI.Entities.Keys;
using Senparc.CO2NET;
using Senparc.AI.AgentKernel.Kernels;
using System.ClientModel;
using Azure.AI.OpenAI;
using Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;

// Memory functionality is experimental
#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020, SKEXP0012, SKEXP0001

namespace Senparc.AI.AgentKernel.Helpers
{

    public partial class AgentKernelHelper
    {
        /* Config* 方法规则：
        1. 相关方法为较底层的调用方法，会直接使用 Semantic Kernel 等模块接口
        2. 所有 modelName、deploymentName，都是用字符串传入，如果留空，则使用 SenparcAiSetting 参数自动获取。
       */



        #region Config



        ///// <summary>
        ///// 设置 Kernel，并配置 TextCompletion 模型
        ///// </summary>
        ///// <param name="userId">用户ID， 用于防止api滥用</param>
        ///// <param name="modelName">模型名 modelId</param>
        ///// <param name="senparcAiSetting"></param>
        ///// <param name="kernelBuilder"></param>
        ///// <returns></returns>
        ///// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        //public IKernelBuilder ConfigTextCompletion(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null,
        //    IKernelBuilder? kernelBuilder = null, string deploymentName = null)
        //{
        //    senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
        //    modelName ??= senparcAiSetting.ModelName.TextCompletion;
        //    deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

        //    var serviceId = GetServiceId(userId, modelName);
        //    var aiPlatForm = senparcAiSetting.AiPlatform;

        //    kernelBuilder ??= Microsoft.SemanticKernel.Kernel.CreateBuilder();

        //    //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

        //    // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
        //    // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法

        //    // use `senparcAiSetting` instead of using `AiSetting` from the config file by default

        //    _ = aiPlatForm switch
        //    {
        //        AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(modelName,
        //                apiKey: senparcAiSetting.ApiKey,
        //                orgId: senparcAiSetting.OrganizationId,
        //                httpClient: _httpClient),
        //        AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
        //                deploymentName: deploymentName,
        //                modelId: modelName,
        //                endpoint: senparcAiSetting.Endpoint,
        //                apiKey: senparcAiSetting.ApiKey,
        //                httpClient: _httpClient),
        //        AiPlatform.NeuCharAI => kernelBuilder.AddAzureOpenAIChatCompletion(
        //                deploymentName: deploymentName,
        //                modelId: modelName,
        //                endpoint: senparcAiSetting.Endpoint,
        //                apiKey: senparcAiSetting.ApiKey,
        //                httpClient: _httpClient),
        //        AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextGeneration(
        //                model: modelName,
        //                apiKey: null,
        //                endpoint: new Uri(senparcAiSetting.Endpoint ?? throw new SenparcAiException("HuggingFace 必须提供 Endpoint")),
        //                serviceId: null,
        //                httpClient: _httpClient),
        //        AiPlatform.FastAPI => kernelBuilder.AddFastAPIChatCompletion(
        //                modelId: modelName,
        //                apiKey: senparcAiSetting.ApiKey,
        //                orgId: senparcAiSetting.OrganizationId,
        //                endpoint: senparcAiSetting.FastAPIEndpoint,
        //                serviceId: null
        //            ),
        //        AiPlatform.Ollama => kernelBuilder.AddOllamaTextGeneration(
        //                modelId: modelName,
        //                endpoint: new Uri(senparcAiSetting.OllamaEndpoint),
        //                serviceId: null
        //            ),
        //        AiPlatform.DeepSeek => kernelBuilder.AddOpenAIChatCompletion(modelName,
        //                endpoint: new Uri(senparcAiSetting.Endpoint),
        //                apiKey: senparcAiSetting.ApiKey,
        //                orgId: senparcAiSetting.OrganizationId,
        //                httpClient: _httpClient),
        //        _ => throw new SenparcAiException($"ConfigTextCompletion 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
        //    };

        //    return kernelBuilder;
        //}



        #endregion
    }
}

#pragma warning restore SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020

