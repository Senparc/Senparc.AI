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
    /// SemanticKernel helper class.
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
        /// <param name="httpClient">When null, automatically builds an <see cref="HttpClient" /> using <see cref="LoggingHttpMessageHandler"/>.</param>
        /// <param name="enableLog">Whether to enable logging for <paramref name="httpClient"/>. This applies only when <paramref name="httpClient"/> is null and a <see cref="LoggingHttpMessageHandler"/> is created automatically.</param>
        public SemanticKernelHelper(ISenparcAiSetting? aiSetting = null, ILoggerFactory? loggerFactory = null, HttpClient httpClient = null, bool enableLog = false)
        {
            AiSetting = aiSetting ?? Senparc.AI.Config.SenparcAiSetting;
            this.loggerFactory = loggerFactory;
            this.ResetHttpClient(httpClient, enableLog);
        }

        /// <summary>
        /// Resets the HttpClient.
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
        /// Gets the chat ServiceId.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public string GetServiceId(string userId, string modelName)
        {
            return $"{userId}-{modelName}";
        }

        /// <summary>
        /// Gets the SemanticKernel object.
        /// </summary>
        /// <param name="kernelBuilderAction">An action to insert before <see cref="KernelBuilder"/> calls <see cref="KernelBuilder.Build()"/>.</param>
        /// <param name="refresh" default="false">Whether to refresh the kernel.</param>
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
        /// Builds a new Kernel object.
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
        /// Resets the SenparcAiSetting parameters.
        /// </summary>
        /// <param name="aiSetting"></param>
        public void ResetSenparcAiSetting(ISenparcAiSetting aiSetting)
        {
            this.AiSetting = aiSetting;
        }

        #region RequestSettings

        /// <summary>
        /// Creates the appropriate ExecutionSettings object for each AiPlatform type.
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
                throw new SenparcAiException("Senparc.AI.Config.SenparcAiSetting is not configured globally. Provide the relevant configuration in the parameters.");
            }

            var aiPlatForm = senparcAiSetting.AiPlatform;
            var chatModelName = senparcAiSetting.ModelName?.Chat;
            var skipSampling = Senparc.AI.Helpers.ModelCapabilityHelper.DoesNotSupportTemperature(chatModelName);

            if (skipSampling)
            {
                System.Console.WriteLine(
                    $"[Debug] Model {chatModelName} does not support sampling parameters such as Temperature or TopP; GetExecutionSetting will ignore these fields.");
            }

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
                _ => skipSampling
                    ? new OpenAIPromptExecutionSettings()
                    {
                        // GPT-5+ and o-series models: do not set Temperature, TopP, PresencePenalty, or FrequencyPenalty.
                        MaxTokens = maxTokens,
                        StopSequences = stopSequences,
                    }
                    : new OpenAIPromptExecutionSettings()
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
        /// Creates the appropriate ExecutionSettings object for each AiPlatform type.
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
