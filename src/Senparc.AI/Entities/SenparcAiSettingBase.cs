using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Entities
{
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
        /// 是否使用 Azure OpenAI
        /// </summary>
        public virtual bool UseAzureOpenAI => AiPlatform == AiPlatform.AzureOpenAI;

        /// <summary>
        /// AI 平台类型
        /// </summary>
        public virtual AiPlatform AiPlatform { get; set; }

        public virtual AzureOpenAIKeys AzureOpenAIKeys { get; set; }
        public virtual OpenAIKeys OpenAIKeys { get; set; }

        /// <summary>
        /// Azure OpenAI 或 OpenAI API Key
        /// </summary>
        public virtual string ApiKey => AiPlatform switch
        {
            AiPlatform.AzureOpenAI => AzureOpenAIKeys.ApiKey,
            AiPlatform.OpenAI => OpenAIKeys.ApiKey,
        };

        /// <summary>
        /// OpenAI API Orgaization ID
        /// </summary>
        public virtual string OrgaizationId => OpenAIKeys.OrgaizationId;

        #region Azure OpenAI

        /// <summary>
        /// Azure OpenAI Endpoint
        /// </summary>
        public virtual string AzureEndpoint => AzureOpenAIKeys.AzureEndpoint;
        /// <summary>
        /// Azure OpenAI 版本号
        /// </summary>
        public virtual string AzureOpenAIApiVersion => AzureOpenAIKeys.AzureOpenAIApiVersion;

        #endregion

        public virtual bool IsOpenAiKeysSetted => OpenAIKeys != null && !OpenAIKeys.ApiKey.IsNullOrEmpty();

    }
}
