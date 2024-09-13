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
using Senparc.AI.Kernel.SparkAI;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using static Humanizer.On;
using System.IO;
using System.Threading;

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
        //public static IKernelBuilder AddFOllamaChatCompletion(this IKernelBuilder builder, string modelId, string endpoint, string serviceId = null)
        //{
        //    string modelId2 = modelId;


        //    Func<IServiceProvider, object, OpenAIChatCompletionService> implementationFactory =
        //        (IServiceProvider serviceProvider, object? _) =>
        //        new OpenAIChatCompletionService(modelId, new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey)), serviceProvider.GetService<ILoggerFactory>());

        //    OllamaApiClientExtensions.

        //    builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, IChatCompletionService>)implementationFactory);
        //    builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, ITextGenerationService>)implementationFactory);
        //    return builder;
        //}
        #region Ollama

        public static IKernelBuilder AddFOllamaChatCompletion(this IKernelBuilder builder, string modelId, string endpoint, string serviceId = null)
        {
            string modelId2 = modelId;

            Func<IServiceProvider, object, OpenAIChatCompletionService> implementationFactory =
    (IServiceProvider serviceProvider, object? _) =>
            new OpenAIChatCompletionService(modelId: modelId, endpoint: new Uri(endpoint), apiKey: "none", loggerFactory: serviceProvider.GetService<ILoggerFactory>());
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, IChatCompletionService>)implementationFactory);
           
            return builder;
        }

        public static IKernelBuilder AddFOllamaTextCompletion(this IKernelBuilder builder, string modelId, string endpoint, string serviceId = null)
        {
            string modelId2 = modelId;

            Func<IServiceProvider, object, OpenAITextGenerationService> implementationFactory =
    (IServiceProvider serviceProvider, object? _) =>
                new OpenAITextGenerationService(modelId: modelId, new OpenAIClient(endpoint: new Uri(endpoint), new Azure.AzureKeyCredential("none")), loggerFactory: serviceProvider.GetService<ILoggerFactory>());

            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, ITextGenerationService>)implementationFactory);

            return builder;
        }

        #endregion
        /// <summary>
        /// 添加讯飞聊天服务
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="modelId"></param>
        /// <param name="apiKey"></param>
        /// <param name="endpoint"></param>
        /// <param name="orgId"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
 public static IKernelBuilder AddSparkAPIChatCompletion(this IKernelBuilder builder, string modelId, string apiKey, string endpoint, string? orgId = null, string? serviceId = null)
        {
            // 创建 HttpClient，用于与讯飞星火 API 交互
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            Func<IServiceProvider, object, OpenAIChatCompletionService> implementationFactory =
                (IServiceProvider serviceProvider, object? _) =>
                new OpenAIChatCompletionService(modelId, new Uri(endpoint), apiKey, null, httpClient);
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, IChatCompletionService>)implementationFactory);
            builder.Services.AddKeyedSingleton((object?)serviceId, (Func<IServiceProvider, object?, ITextGenerationService>)implementationFactory);
            return builder;
        }

        ///// <summary> 
        ///// 添加 SparkAI 聊天服务 现在只有简单聊天 过时
        ///// </summary>
        ///// <param name="builder"></param>
        ///// <param name="modelId"></param>
        ///// <param name="apiKey"></param>
        ///// <param name="endpoint"></param>
        ///// <param name="orgId"></param>
        ///// <param name="serviceId"></param>
        ///// <returns></returns>
        //public static IKernelBuilder AddSparkAIChatCompletion(this IKernelBuilder builder, string appId, string apiKey, string apiSecret,string modelVersion)
        //{
        //    // Register SparkAIService as a singleton in the dependency injection container SparkApiClient
        //    //  builder.Services.AddSingleton<ITextGenerationService>(new SparkAIChatService(appId, apiKey, apiSecret,modelVersion));
        //    builder.Services.AddSingleton<ITextGenerationService>(new SparkApiClient(apiKey, modelVersion));
        //    return builder;
        //}
    }
}
