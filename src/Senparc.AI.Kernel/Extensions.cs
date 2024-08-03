using System;
using System.Collections.Generic;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel;
using OllamaSharp;

namespace Senparc.AI.Kernel
{
    public static class Extensions
    {
        /// <summary>
        /// 添加 FastAPI 聊天服务
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="modelId"></param>
        /// <param name="apiKey"></param>
        /// <param name="endpoint"></param>
        /// <param name="orgId"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static IKernelBuilder AddFastAPIChatCompletion(this IKernelBuilder builder, string modelId, string apiKey, string endpoint, string? orgId = null, string? serviceId = null)
        {
            string modelId2 = modelId;
            string apiKey2 = apiKey;
            string orgId2 = orgId;

            Func<IServiceProvider, object, OpenAIChatCompletionService> implementationFactory = 
                (IServiceProvider serviceProvider, object? _) => 
                new OpenAIChatCompletionService(modelId, new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey)), serviceProvider.GetService<ILoggerFactory>());
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, IChatCompletionService>)implementationFactory);
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, ITextGenerationService>)implementationFactory);
            return builder;
        }

        public static IKernelBuilder AddFOllamaChatCompletion(this IKernelBuilder builder, string modelId, string endpoint, string serviceId=null)
        {
            string modelId2 = modelId;


            Func<IServiceProvider, object, OpenAIChatCompletionService> implementationFactory =
                (IServiceProvider serviceProvider, object? _) =>
                new OpenAIChatCompletionService(modelId, new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey)), serviceProvider.GetService<ILoggerFactory>());

            OllamaApiClientExtensions.

            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, IChatCompletionService>)implementationFactory);
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, ITextGenerationService>)implementationFactory);
            return builder;
        }
    }
}
