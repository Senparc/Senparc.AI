using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Helpers
{
    public partial class SemanticKernelHelper
    {
        /// <summary>
        /// 设置 Kernel，并配置 TextCompletion 模型
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="kernelBuilder"></param>
        /// <returns></returns>
        /// <exception cref="Senparc.AI.Exceptions.SenparcAiException"></exception>
        public IAIKernelBuilder ConfigTextEmbeddingGeneration(string userId, string modelName = null, ISenparcAiSetting senparcAiSetting = null, IAIKernelBuilder? kernelBuilder = null, string deploymentName = null)
        {
            //kernel ??= GetKernel();

            senparcAiSetting ??= this.AiSetting;
            modelName ??= senparcAiSetting.ModelName.Embedding;
            deploymentName ??= senparcAiSetting.DeploymentName ?? modelName;

            var serviceId = GetServiceId(userId, modelName);
            var aiPlatForm = senparcAiSetting.AiPlatform;
            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            //TODO：Builder 不应该新建

            // var kernelBuilder = Microsoft.SemanticKernel.Kernel.Builder;
            // 以上方法已经被SK标注为 Obsolete, 修改为SK推荐的方法
            kernelBuilder ??= Kernels.AIKernelBuilder.CreateBuilder();

            // use `senparcAiSetting` instead of using `AiSetting` from the config file by default
            object _ = aiPlatForm switch
            {
                AiPlatform.OpenAI => kernelBuilder.AddOpenAIEmbedding(
                    apiKey: senparcAiSetting.ApiKey,
                    modelName: modelName),

                AiPlatform.AzureOpenAI => kernelBuilder.AddAzureOpenAIEmbedding(
                    endpoint: new Uri(senparcAiSetting.NeuCharEndpoint),
                    credential: new System.ClientModel.ApiKeyCredential(senparcAiSetting.ApiKey),
                    options: new Azure.AI.OpenAI.AzureOpenAIClientOptions(),
                    azureDeploymentName : deploymentName),
                AiPlatform.NeuCharAI => kernelBuilder.AddNeuCharAIEmbedding(
                    endpoint: new Uri(senparcAiSetting.NeuCharEndpoint),
                    credential: new System.ClientModel.ApiKeyCredential(senparcAiSetting.ApiKey),
                    options: new Azure.AI.OpenAI.AzureOpenAIClientOptions(),
                    modelName: deploymentName),
                //AiPlatform.HuggingFace => kernelBuilder.AddHuggingFaceTextEmbeddingGeneration(
                //    model: modelName,
                //    endpoint: new Uri(senparcAiSetting.Endpoint ?? throw new SenparcAiException("HuggingFace 必须提供 Endpoint")),
                //    httpClient: _httpClient),

                AiPlatform.Ollama => kernelBuilder.AddOllamaEmbedding(
                    endpoint: senparcAiSetting.OllamaEndpoint,
                    modelName: modelName),

                _ => throw new SenparcAiException($"ConfigTextEmbeddingGeneration 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            //TODO:测试多次添加
            //KernelBuilder = builder;
            return kernelBuilder;
        }

        /// <summary>
        /// Get Embedding Result
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="text"></param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="azureDeployName"></param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string modelName, string text, ISenparcAiSetting senparcAiSetting = null, string azureDeployName = null)
        {
            senparcAiSetting ??= this.AiSetting;
            var aiPlatForm = senparcAiSetting.AiPlatform;

            var embeddingService = aiPlatForm switch
            {
                //AiPlatform.OpenAI =>  new OpenAITextEmbeddingGenerationService(
                //                apiKey: senparcAiSetting.ApiKey,
                //                httpClient: _httpClient,
                //                modelId: modelName,
                //                loggerFactory: loggerFactory
                //           );
                // ),

                //memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(
                //    modelId: modelName,
                //    apiKey: senparcAiSetting.ApiKey,
                //    orgId: senparcAiSetting.OrganizationId,
                //    httpClient: _httpClient),
                AiPlatform.AzureOpenAI => new AzureOpenAITextEmbeddingGenerationService(
                           deploymentName: azureDeployName ?? modelName,
                           endpoint: senparcAiSetting.Endpoint,
                                apiKey: senparcAiSetting.ApiKey,
                           httpClient: _httpClient,
                                modelId: modelName,
                                loggerFactory: loggerFactory
                           ),
                AiPlatform.NeuCharAI => new AzureOpenAITextEmbeddingGenerationService(
                            deploymentName: azureDeployName ?? modelName,
                           endpoint: senparcAiSetting.Endpoint,
                                apiKey: senparcAiSetting.ApiKey,
                           httpClient: _httpClient,
                                modelId: modelName,
                                loggerFactory: loggerFactory
                           ),
                //AiPlatform.HuggingFace => memoryBuilder.WithTextEmbeddingGeneration(
                //    (loggerFactory, httpClient) =>
                //    {
                //        return new AzureOpenAITextEmbeddingGenerationService(
                //        deploymentName: azureDeployName,
                //             endpoint: senparcAiSetting.Endpoint,
                //             apiKey: senparcAiSetting.ApiKey,
                //        httpClient: _httpClient,
                //             modelId: modelName,
                //             loggerFactory: loggerFactory
                //        );
                //    }),

                //AiPlatform.Ollama => memoryBuilder.WithTextEmbeddingGeneration((loggerFactory, httpClient) =>
                //{
                //    return new OllamaTextEmbeddingGenerationService(
                //         endpoint: new Uri(senparcAiSetting.Endpoint),
                //         modelId: modelName,
                //         loggerFactory: loggerFactory
                //    );
                //}),

                _ => throw new SenparcAiException($"GetEmbedding 没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
            };

            //var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            try
            {
                var embeddingResult = await embeddingService.GenerateEmbeddingAsync(text, _kernel);

                return embeddingResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Senparc.AIKernel Embedding 出错：");
                Console.WriteLine(ex);
                throw;
            }


        }
    }
}
