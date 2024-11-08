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
using OpenAI;
using System.ClientModel;

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
                new OpenAIChatCompletionService(modelId, new OpenAIClient(new ApiKeyCredential(apiKey)), serviceProvider.GetService<ILoggerFactory>());
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, IChatCompletionService>)implementationFactory);
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, ITextGenerationService>)implementationFactory);
            return builder;
        }

        #region Ollama

        [Obsolete("请使用 AddOllamaChatCompletion", true)]
        public static IKernelBuilder AddFOllamaChatCompletion(this IKernelBuilder builder, string modelId, string endpoint, string serviceId = null)
        {
            return AddOllamaChatCompletion(builder, modelId, endpoint, serviceId);
        }


        public static IKernelBuilder AddOllamaChatCompletion(this IKernelBuilder builder, string modelId, string endpoint, string serviceId = null)
        {
            string modelId2 = modelId;

            Func<IServiceProvider, object, OpenAIChatCompletionService> implementationFactory =
    (IServiceProvider serviceProvider, object? _) =>
            new OpenAIChatCompletionService(modelId: modelId, endpoint: new Uri(endpoint), apiKey: "none", loggerFactory: serviceProvider.GetService<ILoggerFactory>());
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, IChatCompletionService>)implementationFactory);

            return builder;
        }

        [Obsolete("请使用 AddOllamaTextCompletion", true)]
        public static IKernelBuilder AddFOllamaTextCompletion(this IKernelBuilder builder, string modelId, string endpoint, string serviceId = null)
        {
            return AddFOllamaTextCompletion(builder, modelId, endpoint, serviceId);
        }

        public static IKernelBuilder AddOllamaTextCompletion(this IKernelBuilder builder, string modelId, string endpoint, string serviceId = null)
        {
            string modelId2 = modelId;

            Func<IServiceProvider, object, OpenAIChatCompletionService> implementationFactory =
    (IServiceProvider serviceProvider, object? _) =>
            new OpenAIChatCompletionService(modelId: modelId, endpoint: new Uri(endpoint), apiKey: "none", loggerFactory: serviceProvider.GetService<ILoggerFactory>());

            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, ITextGenerationService>)implementationFactory);

            return builder;
        }

        #endregion
    }
}
