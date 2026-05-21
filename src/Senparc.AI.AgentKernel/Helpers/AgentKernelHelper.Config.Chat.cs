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
    public partial class AgentKernelHelper
    {
        /// <summary>
        /// 设置 Kernel，并配置 TextCompletion 模型
        /// </summary>
        /// <param name="userId">用户ID， 用于防止api滥用</param>
        /// <param name="modelName">模型名 modelId</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IAIKernelBuilder ConfigChat(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null,
            IAIKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;
            modelName ??= senparcAiSetting.ModelName.Chat;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();
            kernelBuilder.AddConfigModel(ConfigModel.Chat);

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            kernelBuilder.ChatClient = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIChatCompletion(senparcAiSetting.ApiKey, modelName),
                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIChatCompletion(
                    new Uri(senparcAiSetting.AzureEndpoint),
                    new ApiKeyCredential(senparcAiSetting.ApiKey),
                    new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName
                ),
                AiPlatform.NeuCharAI => kernelBuilder.AddNeuCharAIChatCompletion(
                    new Uri(senparcAiSetting.NeuCharEndpoint),
                    new ApiKeyCredential(senparcAiSetting.ApiKey),
                    new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2025_04_01_Preview),
                    deploymentName: deploymentName
                ),
                //AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextGeneration(
                //        model: modelName,
                //        apiKey: null,
                //        endpoint: new Uri(senparcAiSetting.HuggingFaceEndpoint ?? throw new SenparcAiException("HuggingFace 必须提供 Endpoint")),
                //        serviceId: serviceId,
                //        httpClient: _httpClient),
                //AiPlatform.FastAPI => kernelBuilder.AddFastAPIChatCompletion(
                //        modelId: modelName,
                //        apiKey: senparcAiSetting.ApiKey,
                //        orgId: senparcAiSetting.OrganizationId,
                //        endpoint: senparcAiSetting.FastAPIEndpoint,
                //        serviceId: null
                //    ),
                AiPlatform.Ollama => kernelBuilder.AddOllamaChatCompletion(senparcAiSetting.OllamaEndpoint, modelName),
                //DeepSeek 使用和 OpenAI 一致的请求格式
                AiPlatform.DeepSeek => kernelBuilder.AddOpenAIChatCompletion(senparcAiSetting.ApiKey, modelName),

                _ => throw new SenparcAiException($"ConfigChat 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            return kernelBuilder;
        }
    }
}
