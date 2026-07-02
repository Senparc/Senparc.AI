using Senparc.AI.Entities.Keys;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.CO2NET;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Entities
{
    /// <summary>
    /// SenparcAiSetting<T> base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract record class SenparcAiSettingBase<T> : SenparcAiSettingBase, ISenparcAiSetting<T>
        where T : ISenparcAiSetting
    {
        /// <summary>
        /// Multi-level configuration for different models
        /// </summary>
        public virtual ConcurrentDictionary<string, T> Items { get; set; } = new ConcurrentDictionary<string, T>();

        /// <summary>
        /// Get custom configuration
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T this[string key]
        {
            get => Items[key];
            set => Items[key] = value;
        }
    }

    /// <summary>
    /// SenparcAiSetting base class
    /// </summary>
    public record class SenparcAiSettingBase : ISenparcAiSetting
    {
        /// <summary>
        /// Whether debug mode is enabled
        /// </summary>
        public virtual bool IsDebug { get; set; }

        /// <summary>
        /// Vector database configuration
        /// </summary>
        public VectorDB VectorDB { get; set; }

        /// <summary>
        /// MCP Server configuration
        /// </summary>
        public List<McpServerOption> McpServers { get; set; } = new List<McpServerOption>();

        /// <summary>
        /// Whether OpenAI is used
        /// </summary>
        public virtual bool UseOpenAI => AiPlatform == AiPlatform.OpenAI;

        /// <summary>
        /// Whether Azure OpenAI is used
        /// </summary>
        public virtual bool UseAzureOpenAI => AiPlatform == AiPlatform.AzureOpenAI;

        /// <summary>
        /// Whether NeuCharAI is used
        /// </summary>
        public virtual bool UseNeuCharAI => AiPlatform == AiPlatform.NeuCharAI;

        /// <summary>
        /// Whether HuggingFace is used
        /// </summary>
        public virtual bool UseHuggingFace => AiPlatform == AiPlatform.HuggingFace;

        /// <summary>
        /// Whether FastAPI is used
        /// </summary>
        public virtual bool UseFastAPI => AiPlatform == AiPlatform.FastAPI;

        /// <summary>
        /// Whether Ollama is used
        /// </summary>
        public virtual bool Ollama => AiPlatform == AiPlatform.Ollama;

        /// <summary>
        /// Whether DeepSeek is used
        /// </summary>
        public virtual bool UseDeepSeek => AiPlatform == AiPlatform.DeepSeek;

        /// <summary>
        /// Whether Anthropic is used
        /// </summary>
        public virtual bool UseAnthropic => AiPlatform == AiPlatform.Anthropic;

        /// <summary>
        /// Whether Gemini is used
        /// </summary>
        public virtual bool UseGemini => AiPlatform == AiPlatform.Gemini;

        /// <summary>
        /// Whether Qwen is used (OpenAI-compatible)
        /// </summary>
        public virtual bool UseQwen => AiPlatform == AiPlatform.Qwen;

        /// <summary>
        /// Whether Kimi is used (OpenAI-compatible)
        /// </summary>
        public virtual bool UseKimi => AiPlatform == AiPlatform.Kimi;

        /// <summary>
        /// Whether XunFei is used (OpenAI-compatible)
        /// </summary>
        public virtual bool UseXunFei => AiPlatform == AiPlatform.XunFei;

        /// <summary>
        /// AI platform type
        /// </summary>
        public virtual AiPlatform AiPlatform { get; set; }

        public virtual OpenAIKeys OpenAIKeys { get; set; }
        public virtual NeuCharAIKeys NeuCharAIKeys { get; set; }
        //[Obsolete("Deprecated soon")]
        //public virtual NeuCharAIKeys NeuCharOpenAIKeys { get; set; }
        public virtual AzureOpenAIKeys AzureOpenAIKeys { get; set; }
        public virtual HuggingFaceKeys HuggingFaceKeys { get; set; }

        public virtual FastAPIKeys FastAPIKeys { get; set; }
        public virtual OllamaKeys OllamaKeys { get; set; }

        public virtual DeepSeekKeys DeepSeekKeys { get; set; }
        public virtual AnthropicKeys AnthropicKeys { get; set; }
        public virtual GeminiKeys GeminiKeys { get; set; }
        public virtual QwenKeys QwenKeys { get; set; }
        public virtual KimiKeys KimiKeys { get; set; }
        public virtual XunFeiKeys XunFeiKeys { get; set; }

        /// <summary>
        /// Azure OpenAI or OpenAI API key
        /// </summary>
        public virtual string ApiKey => AiPlatform switch
        {
            AiPlatform.OpenAI => OpenAIKeys?.ApiKey,
            AiPlatform.NeuCharAI => NeuCharAIKeys?.ApiKey,
            AiPlatform.AzureOpenAI => AzureOpenAIKeys?.ApiKey,
            AiPlatform.HuggingFace => HuggingFaceKeys?.ApiKey,
            AiPlatform.FastAPI => FastAPIKeys?.ApiKey,
            AiPlatform.Ollama => "",
            AiPlatform.DeepSeek => DeepSeekKeys?.ApiKey,
            AiPlatform.Anthropic => AnthropicKeys?.ApiKey,
            AiPlatform.Gemini => GeminiKeys?.ApiKey,
            AiPlatform.Qwen => QwenKeys?.ApiKey,
            AiPlatform.Kimi => KimiKeys?.ApiKey,
            AiPlatform.XunFei => XunFeiKeys?.ApiKey,
            _ => ""
        };

        /// <summary>
        /// OpenAI API Orgaization ID
        /// </summary>
        public virtual string OrganizationId => AiPlatform switch
        {
            AiPlatform.OpenAI => OpenAIKeys?.OrganizationId,
            _ => null
        };

        /// <summary>
        /// Anthropic API version header value.
        /// </summary>
        public virtual string AnthropicVersion => AnthropicKeys?.AnthropicVersion;

        #region OpenAI

        /// <summary>
        /// OpenAI Endpoint
        /// </summary>
        public virtual string OpenAIEndpoint => OpenAIKeys?.OpenAIEndpoint;

        #endregion

        #region Azure OpenAI

        /// <summary>
        /// Azure OpenAI Endpoint
        /// </summary>
        public virtual string AzureEndpoint => AzureOpenAIKeys?.AzureEndpoint;

        /// <summary>
        /// Azure OpenAI version
        /// </summary>
        public virtual string AzureOpenAIApiVersion => AzureOpenAIKeys?.AzureOpenAIApiVersion;

        #endregion


        #region NeuChar

        /// <summary>
        /// NeuChar Endpoint
        /// </summary>
        public virtual string NeuCharEndpoint => NeuCharAIKeys?.NeuCharEndpoint;

        /// <summary>
        /// Azure OpenAI version
        /// </summary>
        //[Obsolete("Deprecated. Use NeuCharAIApiVersion", true)]
        //public virtual string NeuCharOpenAIApiVersion => NeuCharAIKeys?.NeuCharAIApiVersion;
        /// <summary>
        /// Azure OpenAI version
        /// </summary>
        public virtual string NeuCharAIApiVersion => NeuCharAIKeys?.NeuCharAIApiVersion;

        #endregion

        #region HuggingFace

        public virtual string HuggingFaceEndpoint => HuggingFaceKeys?.Endpoint;

        #endregion

        #region FastAPI
        public string FastAPIEndpoint => FastAPIKeys?.Endpoint;

        #endregion

        #region Ollama
        public string OllamaEndpoint => OllamaKeys?.Endpoint;

        #endregion

        #region DeepSeek
        public virtual string DeepSeekEndpoint => DeepSeekKeys?.Endpoint;

        #endregion

        #region Anthropic
        public virtual string AnthropicEndpoint => AnthropicKeys?.Endpoint;

        #endregion

        #region Gemini
        public virtual string GeminiEndpoint => GeminiKeys?.Endpoint;

        #endregion

        #region Qwen
        public virtual string QwenEndpoint => QwenKeys?.Endpoint;

        #endregion

        #region Kimi
        public virtual string KimiEndpoint => KimiKeys?.Endpoint;

        #endregion

        #region XunFei
        public virtual string XunFeiEndpoint => XunFeiKeys?.Endpoint;

        #endregion

        public virtual bool IsMcpServersSetted => McpServers != null && McpServers.Count > 0;

       public virtual string Endpoint => AiPlatform switch
       {
        AiPlatform.OpenAI => OpenAIEndpoint,
        AiPlatform.AzureOpenAI => AzureEndpoint,
        AiPlatform.NeuCharAI => NeuCharEndpoint,
        AiPlatform.HuggingFace => HuggingFaceEndpoint,
        AiPlatform.FastAPI => FastAPIEndpoint,
        AiPlatform.Ollama => OllamaEndpoint,
        AiPlatform.DeepSeek => DeepSeekEndpoint,
        AiPlatform.Anthropic => AnthropicEndpoint,
        AiPlatform.Gemini => GeminiEndpoint,
        AiPlatform.Qwen => QwenEndpoint,
        AiPlatform.Kimi => KimiEndpoint,
        AiPlatform.XunFei => XunFeiEndpoint,
        _ => throw new SenparcAiException($"Endpoint output is not configured for {AiPlatform}")
       };

        public virtual bool IsOpenAiKeysSetted => OpenAIKeys != null && !OpenAIKeys.ApiKey.IsNullOrEmpty();

        public SenparcAiSettingBase()
        {
            VectorDB = new VectorDB() { Type = VectorDBType.Default };
            McpServers = new List<McpServerOption>();
        }

        #region Quick configuration methods

        /// <summary>
        /// Set OpenAI
        /// </summary>
        /// <param name="openAIKeys"></param>
        public ISenparcAiSetting SetOpenAI(OpenAIKeys openAIKeys)
        {
            this.AiPlatform = AiPlatform.OpenAI;
            this.OpenAIKeys = openAIKeys;
            return this;
        }

        ///<summary>
        /// Set AzureOpenAI
        /// </summary>
        public ISenparcAiSetting SetAzureOpenAI(AzureOpenAIKeys azureOpenAIKeys)
        {
            this.AiPlatform = AiPlatform.AzureOpenAI;
            this.AzureOpenAIKeys = azureOpenAIKeys;
            return this;
        }

        /// <summary>
        /// Set NeuCharAI
        /// </summary>
        /// <param name="neuCharAIKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetNeuCharAI(NeuCharAIKeys neuCharAIKeys)
        {
            this.AiPlatform = AiPlatform.NeuCharAI;
            this.NeuCharAIKeys = neuCharAIKeys;
            return this;
        }

        /// <summary>
        /// Set HuggingFace
        /// </summary>
        /// <param name="huggingFaceKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetHuggingFace(HuggingFaceKeys huggingFaceKeys)
        {
            this.AiPlatform = AiPlatform.HuggingFace;
            this.HuggingFaceKeys = huggingFaceKeys;
            return this;
        }


        /// <summary>
        /// Set FastAPIKeys
        /// </summary>
        /// <param name="fastAPIKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetFastAPI(FastAPIKeys fastAPIKeys)
        {
            this.AiPlatform = AiPlatform.FastAPI;
            this.FastAPIKeys = fastAPIKeys;
            return this;
        }

        /// <summary>
        /// Set Ollama
        /// </summary>
        /// <param name="ollamaAPIKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetOllama(OllamaKeys ollamaAPIKeys)
        {
            this.AiPlatform = AiPlatform.Ollama;
            this.OllamaKeys = ollamaAPIKeys;
            return this;
        }

        /// <summary>
        /// Set DeepSeek
        /// </summary>
        /// <param name="deepSeekKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetDeepSeek(DeepSeekKeys deepSeekKeys)
        {
            this.AiPlatform = AiPlatform.DeepSeek;
            this.DeepSeekKeys = deepSeekKeys;
            return this;
        }

        /// <summary>
        /// Set Anthropic
        /// </summary>
        /// <param name="anthropicKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetAnthropic(AnthropicKeys anthropicKeys)
        {
            this.AiPlatform = AiPlatform.Anthropic;
            this.AnthropicKeys = anthropicKeys;
            return this;
        }

        /// <summary>
        /// Set Gemini
        /// </summary>
        /// <param name="geminiKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetGemini(GeminiKeys geminiKeys)
        {
            this.AiPlatform = AiPlatform.Gemini;
            this.GeminiKeys = geminiKeys;
            return this;
        }

        /// <summary>
        /// Set Qwen (OpenAI-compatible)
        /// </summary>
        /// <param name="qwenKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetQwen(QwenKeys qwenKeys)
        {
            this.AiPlatform = AiPlatform.Qwen;
            this.QwenKeys = qwenKeys;
            return this;
        }

        /// <summary>
        /// Set Kimi (OpenAI-compatible)
        /// </summary>
        /// <param name="kimiKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetKimi(KimiKeys kimiKeys)
        {
            this.AiPlatform = AiPlatform.Kimi;
            this.KimiKeys = kimiKeys;
            return this;
        }

        /// <summary>
        /// Set XunFei (OpenAI-compatible)
        /// </summary>
        /// <param name="xunFeiKeys"></param>
        /// <returns></returns>
        public ISenparcAiSetting SetXunFei(XunFeiKeys xunFeiKeys)
        {
            this.AiPlatform = AiPlatform.XunFei;
            this.XunFeiKeys = xunFeiKeys;
            return this;
        }

        /// <summary>
        /// Set another platform
        /// </summary>
        /// <returns></returns>
        public ISenparcAiSetting SetOtherPlatform()
        {
            this.AiPlatform = AiPlatform.Other;
            return this;
        }

        #endregion


        public ModelName ModelName => AiPlatform switch
        {
            AiPlatform.OpenAI => OpenAIKeys.ModelName,
            AiPlatform.AzureOpenAI => AzureOpenAIKeys.ModelName,
            AiPlatform.NeuCharAI => NeuCharAIKeys.ModelName,
            AiPlatform.HuggingFace => HuggingFaceKeys.ModelName,
            AiPlatform.FastAPI => FastAPIKeys.ModelName,
            AiPlatform.Ollama => OllamaKeys.ModelName,
            AiPlatform.DeepSeek => DeepSeekKeys.ModelName,
            AiPlatform.Anthropic => AnthropicKeys.ModelName,
            AiPlatform.Gemini => GeminiKeys.ModelName,
            AiPlatform.Qwen => QwenKeys.ModelName,
            AiPlatform.Kimi => KimiKeys.ModelName,
            AiPlatform.XunFei => XunFeiKeys.ModelName,
            _ => throw new SenparcAiException($"ModelName output is not configured for {AiPlatform}")
        };

#pragma warning disable CS8603 // Possible null reference return.
        public string DeploymentName => AiPlatform switch
        {
            AiPlatform.AzureOpenAI => AzureOpenAIKeys.DeploymentName,
            AiPlatform.OpenAI => null,
            AiPlatform.NeuCharAI => null,
            AiPlatform.HuggingFace => null,
            AiPlatform.FastAPI => null,
            AiPlatform.Ollama => null,
            AiPlatform.DeepSeek => null,
            AiPlatform.Anthropic => null,
            AiPlatform.Gemini => null,
            AiPlatform.Qwen => null,
            AiPlatform.Kimi => null,
            AiPlatform.XunFei => null,
            _ => throw new SenparcAiException($"DeploymentName output is not configured for {AiPlatform}")
        };

    }
}
