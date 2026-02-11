using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.HttpMessageHandlers;
using Senparc.CO2NET;

// Memory functionality is experimental
#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020

namespace Senparc.AI.Kernel.Helpers
{
    /// <summary>
    /// SemanticKernel 帮助类
    /// </summary>
    public partial class SemanticKernelHelper
    {
        public ISemanticTextMemory? SemanticTextMemory { get; set; }

        private Microsoft.SemanticKernel.Kernel _kernel { get; set; }

        internal IKernelBuilder KernelBuilder { get; set; } = Microsoft.SemanticKernel.Kernel.CreateBuilder();

        public ISenparcAiSetting AiSetting { get; private set; }

        private List<Task> _memoryExecuteList = new List<Task>();
        private readonly ILoggerFactory? loggerFactory;

        //private LoggingHttpMessageHandler _httpHandler;
        public HttpClient _httpClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aiSetting"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="httpClient">为 null 时，自动使用 <see cref="LoggingHttpMessageHandler"/> 构建 <see cref="HttpClient" /></param>
        /// <param name="enableLog">是否开启 <paramref name="httpClient"/> 的日志（仅在 <paramref name="httpClient"/> 为 null 时，会自动构建 <see cref="LoggingHttpMessageHandler"/> 时生效。</param>
        public SemanticKernelHelper(ISenparcAiSetting? aiSetting = null, ILoggerFactory? loggerFactory = null, HttpClient httpClient = null, bool enableLog = false)
        {
            AiSetting = aiSetting ?? Senparc.AI.Config.SenparcAiSetting;
            this.loggerFactory = loggerFactory;
            this.ResetHttpClient(httpClient, enableLog);
        }

        /// <summary>
        /// 重置 HttpClient
        /// </summary>
        /// <param name="httpClient"></param>
        public void ResetHttpClient(HttpClient httpClient = null, bool enableLog = false)
        {
            var builder = new HttpMessageHandlerBuilder();

            var handler = new HttpClientHandler();

            builder.Add(new LoggingHttpMessageHandler(handler, enableLog));
            builder.Add(new RedirectingHttpMessageHandler(handler, AiSetting));

            _httpClient = httpClient ?? new HttpClient(builder.Build());
        }

        /// <summary>
        /// 获取对话 ServiceId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public string GetServiceId(string userId, string modelName)
        {
            return $"{userId}-{modelName}";
        }

        /// <summary>
        /// 获取 SemanticKernel 对象
        /// </summary>
        /// <param name="kernelBuilderAction"><see cref="KernelBuilder"/> 在进行 <see cref="KernelBuilder.Build()"/> 之前需要插入的操作</param>
        /// <param name="refresh" default="false">是否需要刷新kernel</param>
        /// <returns></returns>
        public Microsoft.SemanticKernel.Kernel GetKernel(Action<IKernelBuilder>? kernelBuilderAction = null, bool refresh = false)
        {
            if (_kernel != null && !refresh)
            {
                return _kernel;
            }

            return BuildKernel(KernelBuilder, kernelBuilderAction);
        }


        /// <summary>
        /// Build 新的 Kernel 对象
        /// </summary>
        /// <param name="kernelBuilder"></param>
        /// <param name="kernelBuilderAction"></param>
        /// <returns></returns>
        public Microsoft.SemanticKernel.Kernel BuildKernel(IKernelBuilder kernelBuilder, Action<IKernelBuilder>? kernelBuilderAction = null)
        {
            kernelBuilderAction?.Invoke(kernelBuilder);

            if (loggerFactory != null)
            {
                kernelBuilder.Services.AddSingleton(loggerFactory);
            }

            _kernel = kernelBuilder.Build();
            return _kernel;
        }

        /// <summary>
        /// 重新设置 SenparcAiSetting 参数
        /// </summary>
        /// <param name="aiSetting"></param>
        public void ResetSenparcAiSetting(ISenparcAiSetting aiSetting)
        {
            this.AiSetting = aiSetting;
        }

        #region RequestSettings

        /// <summary>
        /// 根据不同的 AiPlatform 类型生成不同的 ExecutionSettings 对象
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="topP"></param>
        /// <param name="maxTokens"></param>
        /// <param name="presencePenalty"></param>
        /// <param name="frequencyPenalty"></param>
        /// <param name="stopSequences"></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public PromptExecutionSettings GetExecutionSetting(ISenparcAiSetting senparcAiSetting, double temperature = default, double topP = default, int? maxTokens = default, double presencePenalty = default, double frequencyPenalty = default, IList<string>? stopSequences = default)
        {
            senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;

            if (senparcAiSetting == null)
            {
                throw new SenparcAiException("全局未设置 Senparc.AI.Config.SenparcAiSetting，请在参数中提供相关配置！");
            }

            var aiPlatForm = senparcAiSetting.AiPlatform;

            var promptExecutiongSetting = aiPlatForm switch
            {
                //AiPlatform.OpenAI => new OpenAIPromptExecutionSettings()
                //{
                //    Temperature = temperature,
                //    TopP = topP,
                //    MaxTokens = maxTokens,
                //    PresencePenalty = presencePenalty,
                //    FrequencyPenalty = frequencyPenalty,
                //    StopSequences = stopSequences
                //},
                //AiPlatform.AzureOpenAI =>
                //AiPlatform.NeuCharAI => 
                //AiPlatform.HuggingFace => 
                _ => new OpenAIPromptExecutionSettings()
                {
                    Temperature = temperature,
                    TopP = topP,
                    MaxTokens = maxTokens,
                    PresencePenalty = presencePenalty,
                    FrequencyPenalty = frequencyPenalty,
                    StopSequences = stopSequences,
                     
                  
                },
            };

            return promptExecutiongSetting;
        }

        /// <summary>
        /// 根据不同的 AiPlatform 类型生成不同的 ExecutionSettings 对象
        /// </summary>
        /// <param name="promptConfigParameter"></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public PromptExecutionSettings GetExecutionSetting(PromptConfigParameter promptConfigParameter, ISenparcAiSetting senparcAiSetting)
        {
            return GetExecutionSetting(
                   senparcAiSetting: senparcAiSetting,
                   temperature: promptConfigParameter.Temperature ?? default,
                   topP: promptConfigParameter.TopP ?? default,
                   maxTokens: promptConfigParameter.MaxTokens,
                   presencePenalty: promptConfigParameter.PresencePenalty ?? default,
                   frequencyPenalty: promptConfigParameter.FrequencyPenalty ?? default,
                   stopSequences: promptConfigParameter.StopSequences
                    );
        }

        #endregion
    }
}