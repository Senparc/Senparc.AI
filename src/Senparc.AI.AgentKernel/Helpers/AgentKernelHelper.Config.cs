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
        /* Config* method rules:
        1. Related methods are lower-level call methods and directly use module interfaces such as Semantic Kernel
        2. All modelName and deploymentName values are passed as strings. If left empty, they are read automatically from SenparcAiSetting.
       */



        #region Config



        ///// <summary>
        ///// Set Kernel and configure the TextCompletion model
        ///// </summary>
        ///// <param name="userId">User ID, used to prevent API abuse</param>
        ///// <param name="modelName">Model name modelId</param>
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

        //    //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

        //    // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
        //    // The previous method has been marked obsolete by SK. Changed to the method recommended by SK.

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
        //                endpoint: new Uri(senparcAiSetting.Endpoint ?? throw new SenparcAiException("HuggingFace requires Endpoint")),
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
        //        _ => throw new SenparcAiException($"ConfigTextCompletion does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
        //    };

        //    return kernelBuilder;
        //}



        #endregion
    }
}

#pragma warning restore SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020

