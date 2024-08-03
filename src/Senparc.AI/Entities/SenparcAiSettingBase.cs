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
    /// SenparcAiSetting<T> 基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SenparcAiSettingBase<T> : SenparcAiSettingBase, ISenparcAiSetting<T>
        where T : ISenparcAiSetting
    {
        /// <summary>
        /// 多级不同模型配置
        /// </summary>
        public virtual ConcurrentDictionary<string, T> Items { get; set; } = new ConcurrentDictionary<string, T>();

        /// <summary>
        /// 获取自定义配置
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
    /// SenparcAiSetting 基类
    /// </summary>
    public class SenparcAiSettingBase : ISenparcAiSetting
    {
        /// <summary>
        /// 是否处于调试状态
        /// </summary>
        public virtual bool IsDebug { get; set; }

        /// <summary>
        /// 是否使用 OpenAI
        /// </summary>
        public virtual bool UseOpenAI => AiPlatform == AiPlatform.OpenAI;

        /// <summary>
        /// 是否使用 Azure OpenAI
        /// </summary>
        public virtual bool UseAzureOpenAI => AiPlatform == AiPlatform.AzureOpenAI;

        /// <summary>
        /// 是否使用 NeuCharAI
        /// </summary>
        public virtual bool UseNeuCharAI => AiPlatform == AiPlatform.NeuCharAI;

        /// <summary>
        /// 是否使用 HuggingFace
        /// </summary>
        public virtual bool UseHuggingFace => AiPlatform == AiPlatform.HuggingFace;

        /// <summary>
        /// 是否使用 FastAPI
        /// </summary>
        public virtual bool UseFastAPI => AiPlatform == AiPlatform.FastAPI;

        /// <summary>
        /// 是否使用 Ollama
        /// </summary>
        public virtual bool Ollama => AiPlatform == AiPlatform.Ollama;

        /// <summary>
        /// AI 平台类型
        /// </summary>
        public virtual AiPlatform AiPlatform { get; set; }

        public virtual OpenAIKeys OpenAIKeys { get; set; }
        public virtual NeuCharAIKeys NeuCharAIKeys { get; set; }
        //[Obsolete("即将过期")]
        //public virtual NeuCharAIKeys NeuCharOpenAIKeys { get; set; }
        public virtual AzureOpenAIKeys AzureOpenAIKeys { get; set; }
        public virtual HuggingFaceKeys HuggingFaceKeys { get; set; }

        public virtual FastAPIKeys FastAPIKeys { get; set; }
        public virtual OllamaKeys OllamaKeys { get; set; }

        /// <summary>
        /// Azure OpenAI 或 OpenAI API Key
        /// </summary>
        public virtual string ApiKey => AiPlatform switch
        {
            AiPlatform.OpenAI => OpenAIKeys.ApiKey,
            AiPlatform.NeuCharAI => NeuCharAIKeys.ApiKey,
            AiPlatform.AzureOpenAI => AzureOpenAIKeys.ApiKey,
            AiPlatform.HuggingFace => "",
            AiPlatform.FastAPI => FastAPIKeys.ApiKey,
            AiPlatform.Ollama => "",
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
        /// Azure OpenAI 版本号
        /// </summary>
        public virtual string AzureOpenAIApiVersion => AzureOpenAIKeys?.AzureOpenAIApiVersion;

        #endregion


        #region NeuChar

        /// <summary>
        /// NeuChar Endpoint
        /// </summary>
        public virtual string NeuCharEndpoint => NeuCharAIKeys?.NeuCharEndpoint;

        /// <summary>
        /// Azure OpenAI 版本号
        /// </summary>
        //[Obsolete("已过期，请使用 NeuCharAIApiVersion", true)]
        //public virtual string NeuCharOpenAIApiVersion => NeuCharAIKeys?.NeuCharAIApiVersion;
        /// <summary>
        /// Azure OpenAI 版本号
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

        public virtual bool IsOpenAiKeysSetted => OpenAIKeys != null && !OpenAIKeys.ApiKey.IsNullOrEmpty();


        #region 快速配置方法

        /// <summary>
        /// 设置 OpenAI
        /// </summary>
        /// <param name="openAIKeys"></param>
        public ISenparcAiSetting SetOpenAI(OpenAIKeys openAIKeys)
        {
            this.AiPlatform = AiPlatform.OpenAI;
            this.OpenAIKeys = openAIKeys;
            return this;
        }

        ///<summary>
        /// 设置 AzureOpenAI
        /// </summary>
        public ISenparcAiSetting SetAzureOpenAI(AzureOpenAIKeys azureOpenAIKeys)
        {
            this.AiPlatform = AiPlatform.AzureOpenAI;
            this.AzureOpenAIKeys = azureOpenAIKeys;
            return this;
        }

        /// <summary>
        /// 设置 NeuCharAI
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
        /// 设置 HuggingFace
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
        /// 设置 FastAPIKeys
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
        /// 设置 Ollama
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
        /// 设置其他平台
        /// </summary>
        /// <returns></returns>
        public ISenparcAiSetting SetOtherPlatform()
        {
            this.AiPlatform = AiPlatform.Other;
            return this;
        }

        #endregion
    }
}