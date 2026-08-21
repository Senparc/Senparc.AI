using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Senparc.AI.AgentKernel.HttpMessageHandlers;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.CO2NET;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

// Memory functionality is experimental
#pragma warning disable SKEXP0003, SKEXP0011, SKEXP0052, SKEXP0020

namespace Senparc.AI.AgentKernel.Helpers
{
    /// <summary>
    /// SemanticKernel helper class
    /// </summary>
    public partial class AgentKernelHelper
    {
        //public ISemanticTextMemory? SemanticTextMemory { get; set; }

        private AiKernel _kernel { get; set; }

        internal IAIKernelBuilder KernelBuilder { get; set; } = Kernels.AIKernelBuilder.CreateBuilder();

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
        /// <param name="httpClient">When null, automatically build <see cref="HttpClient" /> with <see cref="LoggingHttpMessageHandler"/></param>
        /// <param name="enableLog">Whether to enable logging for <paramref name="httpClient"/>. Effective only when <paramref name="httpClient"/> is null and <see cref="LoggingHttpMessageHandler"/> is automatically built.</param>
        public AgentKernelHelper(ISenparcAiSetting? aiSetting = null, ILoggerFactory? loggerFactory = null, HttpClient httpClient = null, bool enableLog = false)
        {
            AiSetting = aiSetting ?? Senparc.AI.Config.SenparcAiSetting;
            this.loggerFactory = loggerFactory;
            this.ResetHttpClient(httpClient, enableLog);
        }

        /// <summary>
        /// Reset HttpClient
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
        /// Get conversation ServiceId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public string GetServiceId(string userId, string modelName)
        {
            return $"{userId}-{modelName}";
        }

        /// <summary>
        /// Get the SemanticKernel object
        /// </summary>
        /// <param name="kernelBuilderAction">Operations to insert into <see cref="KernelBuilder"/> before <see cref="AIKernelBuilder.Build()"/></param>
        /// <param name="refresh" default="false">Whether the kernel needs to be refreshed</param>
        /// <returns></returns>
        public AiKernel GetKernel(ChatClientAgentOptions chatClientAgentOptions = null, Action<IAIKernelBuilder>? kernelBuilderAction = null, bool refresh = false)
        {
            if (_kernel != null && !refresh)
            {
                return _kernel;
            }

            return  BuildKernel(KernelBuilder, chatClientAgentOptions, kernelBuilderAction);
        }


        /// <summary>
        /// Build a new Kernel object
        /// </summary>
        /// <param name="kernelBuilder"></param>
        /// <param name="kernelBuilderAction"></param>
        /// <returns></returns>
        public AiKernel BuildKernel(IAIKernelBuilder kernelBuilder, ChatClientAgentOptions chatClientAgentOptions = null, Action<IAIKernelBuilder>? kernelBuilderAction = null)
        {
            kernelBuilderAction?.Invoke(kernelBuilder);

            if (loggerFactory != null)
            {
                kernelBuilder.Services.AddSingleton(loggerFactory);
            }

            _kernel = kernelBuilder.Build(AiSetting, chatClientAgentOptions);
            return _kernel;
        }

        /// <summary>
        /// Reset SenparcAiSetting parameters
        /// </summary>
        /// <param name="aiSetting"></param>
        public void ResetSenparcAiSetting(ISenparcAiSetting aiSetting)
        {
            this.AiSetting = aiSetting;
        }

        #region RequestSettings

        ///// <summary>
        ///// Generate different ExecutionSettings objects for different AiPlatform types
        ///// </summary>
        ///// <param name="temperature"></param>
        ///// <param name="topP"></param>
        ///// <param name="maxTokens"></param>
        ///// <param name="presencePenalty"></param>
        ///// <param name="frequencyPenalty"></param>
        ///// <param name="stopSequences"></param>
        ///// <param name="senparcAiSetting"></param>
        ///// <returns></returns>
        //public PromptExecutionSettings GetExecutionSetting(ISenparcAiSetting senparcAiSetting, double temperature = default, double topP = default, int? maxTokens = default, double presencePenalty = default, double frequencyPenalty = default, IList<string>? stopSequences = default)
        //{
        //    senparcAiSetting ??= Senparc.AI.Config.SenparcAiSetting;

        //    if (senparcAiSetting == null)
        //    {
        //        throw new SenparcAiException("Global Senparc.AI.Config.SenparcAiSetting is not set. Provide the relevant configuration in the parameters.");
        //    }

        //    var aiPlatForm = senparcAiSetting.AiPlatform;

        //    var promptExecutiongSetting = aiPlatForm switch
        //    {
        //        //AiPlatform.OpenAI => new OpenAIPromptExecutionSettings()
        //        //{
        //        //    Temperature = temperature,
        //        //    TopP = topP,
        //        //    MaxTokens = maxTokens,
        //        //    PresencePenalty = presencePenalty,
        //        //    FrequencyPenalty = frequencyPenalty,
        //        //    StopSequences = stopSequences
        //        //},
        //        //AiPlatform.AzureOpenAI =>
        //        //AiPlatform.NeuCharAI => 
        //        //AiPlatform.HuggingFace => 
        //        _ => new OpenAIPromptExecutionSettings()
        //        {
        //            Temperature = temperature,
        //            TopP = topP,
        //            MaxTokens = maxTokens,
        //            PresencePenalty = presencePenalty,
        //            FrequencyPenalty = frequencyPenalty,
        //            StopSequences = stopSequences,
        //        },
        //    };

        //    return promptExecutiongSetting;
        //}

        ///// <summary>
        ///// Generate different ExecutionSettings objects for different AiPlatform types
        ///// </summary>
        ///// <param name="promptConfigParameter"></param>
        ///// <param name="senparcAiSetting"></param>
        ///// <returns></returns>
        //public PromptExecutionSettings GetExecutionSetting(PromptConfigParameter promptConfigParameter, ISenparcAiSetting senparcAiSetting)
        //{
        //    return GetExecutionSetting(
        //           senparcAiSetting: senparcAiSetting,
        //           temperature: promptConfigParameter.Temperature ?? default,
        //           topP: promptConfigParameter.TopP ?? default,
        //           maxTokens: promptConfigParameter.MaxTokens,
        //           presencePenalty: promptConfigParameter.PresencePenalty ?? default,
        //           frequencyPenalty: promptConfigParameter.FrequencyPenalty ?? default,
        //           stopSequences: promptConfigParameter.StopSequences
        //            );
        //}

        #endregion
    }
}