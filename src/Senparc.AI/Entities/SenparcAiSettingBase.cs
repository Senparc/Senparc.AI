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
    /// Base class for SenparcAiSetting<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract record class SenparcAiSettingBase<T> : SenparcAiSettingBase, ISenparcAiSetting<T>
        where T : ISenparcAiSetting
    {
        /// <summary>
        /// Multi-level model configuration
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
    /// Base class for SenparcAiSetting
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
        /// AI platform type
        /// </summary>
        public virtual AiPlatform AiPlatform { get; set; }

        public virtual OpenAIKeys OpenAIKeys { get; set; }
        public virtual NeuCharAIKeys NeuCharAIKeys { get; set; }
        //[Obsolete("Will expire soon")]
        //public virtual NeuCharAIKeys NeuCharOpenAIKeys { get; set; }
        public virtual AzureOpenAIKeys AzureOpenAIKeys { get; set; }
        public virtual HuggingFaceKeys HuggingFaceKeys { get; set; }

        public virtual FastAPIKeys FastAPIKeys { get; set; }
        public virtual OllamaKeys OllamaKeys { get; set; }

        public virtual DeepSeekKeys DeepSeekKeys { get; set; }

        /// <summary>
        /// Azure OpenAI or OpenAI API key
        /// </summary>
        public virtual string ApiKey => AiPlatform switch
        {
            AiPlatform.OpenAI => OpenAIKeys?.ApiKey,
            AiPlatform.NeuCharAI => NeuCharAIKeys?.ApiKey,
            AiPlatform.AzureOpenAI => AzureOpenAIKeys?.ApiKey,
            AiPlatform.HuggingFace => "",
            AiPlatform.FastAPI => FastAPIKeys.ApiKey,
            AiPlatform.Ollama => "",
            AiPlatform.DeepSeek => DeepSeekKeys?.ApiKey,
            _ => ""
        };

        /// <summary>
        /// OpenAI API Orgaization ID
        /// </summary>
        public virtual string OrganizationId => AiPlatform == AiPlatform.OpenAI
                                                    ? OpenAIKeys?.OrganizationId
                                                    : FastAPIKeys?.OrganizationId;

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
        //[Obsolete("Expired. Use NeuCharAIApiVersion.", true)]
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
        public string DeepSeekEndpoint => DeepSeekKeys?.Endpoint;

        #endregion

        public virtual bool IsOpenAiKeysSetted => OpenAIKeys != null && !OpenAIKeys.ApiKey.IsNullOrEmpty();

        public SenparcAiSettingBase()
        {
            VectorDB = new VectorDB() { Type = VectorDBType.Default };
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
            _ => throw new SenparcAiException($"not configured {AiPlatform}  ModelName output")
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
            _ => throw new SenparcAiException($"not configured {AiPlatform}  DeploymentName output")
        };

    }
}