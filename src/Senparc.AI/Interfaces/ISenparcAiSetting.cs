using Senparc.AI.Entities;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// Senparc.AI 基础配置，附带 Items 设置多模型配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISenparcAiSetting<T> where T : ISenparcAiSetting
    {
        ConcurrentDictionary<string, T> Items { get; set; }

        T this[string key]
        {
            get => Items[key];
            set => Items[key] = value;
        }
    }

    /// <summary>
    /// Senparc.AI 基础配置
    /// </summary>
    public interface ISenparcAiSetting
    {
        /// <summary>
        /// 是否处于调试状态
        /// </summary>
        bool IsDebug { get; set; }

        /// <summary>
        /// 是否使用 Azure OpenAI
        /// </summary>
        bool UseAzureOpenAI => AiPlatform == AiPlatform.AzureOpenAI;

        /// <summary>
        /// 是否使用 NeuChar OpenAI
        /// </summary>
        bool UseNeuCharAI => AiPlatform == AiPlatform.NeuCharAI;
        /// <summary>
        /// AI 平台类型
        /// </summary>
        AiPlatform AiPlatform { get; set; }


        AzureOpenAIKeys AzureOpenAIKeys { get; set; }
        NeuCharAIKeys NeuCharAIKeys { get; set; }
        OpenAIKeys OpenAIKeys { get; set; }
        HuggingFaceKeys HuggingFaceKeys { get; set; }

        /// <summary>
        /// Neuchar OpenAI 或 Azure OpenAI 或 OpenAI API Key
        /// </summary>
        string ApiKey { get; }

        /// <summary>
        /// OpenAI API Orgaization ID
        /// </summary>
        string OrganizationId { get; }

        #region Azure OpenAI

        /// <summary>
        /// Azure OpenAI Endpoint
        /// </summary>
        string AzureEndpoint { get; }
        /// <summary>
        /// Azure OpenAI 版本号
        /// </summary>
        string AzureOpenAIApiVersion { get; }

        #endregion

        #region NeuChar

        /// <summary>
        /// NeuChar OpenAI Endpoint
        /// </summary>
        string NeuCharEndpoint { get; }
        /// <summary>
        /// Azure OpenAI 版本号
        /// </summary>
        [Obsolete("已过期，请使用 NeuCharAIApiVersion", true)]
        string NeuCharOpenAIApiVersion { get; }

        /// <summary>
        /// 对应 Azure OpenAI 版本号
        /// </summary>
        string NeuCharAIApiVersion { get; }

        #endregion

        #region HuggingFace

        string HuggingFaceEndpoint { get; }

        #endregion

        /// <summary>
        /// OpenAIKeys 是否已经设置
        /// </summary>
        public bool IsOpenAiKeysSetted { get; }
    }
}
