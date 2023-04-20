﻿using Senparc.AI.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Senparc.AI.Interfaces
{
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
        /// AI 平台类型
        /// </summary>
        AiPlatform AiPlatform { get; set; }

        AzureOpenAIKeys AzureOpenAIKeys { get; set; }
        OpenAIKeys OpenAIKeys { get; set; }

        /// <summary>
        /// Azure OpenAI 或 OpenAI API Key
        /// </summary>
        string ApiKey => AiPlatform switch
        {
            AiPlatform.AzureOpenAI => AzureOpenAIKeys.ApiKey,
            AiPlatform.OpenAI => OpenAIKeys.ApiKey,
        };

        /// <summary>
        /// OpenAI API Orgaization ID
        /// </summary>
        string OrgaizationId => OpenAIKeys.OrgaizationId;

        #region Azure OpenAI

        /// <summary>
        /// Azure OpenAI Endpoint
        /// </summary>
        string AzureEndpoint => AzureOpenAIKeys.AzureEndpoint;
        /// <summary>
        /// Azure OpenAI 版本号
        /// </summary>
        string AzureOpenAIApiVersion => AzureOpenAIKeys.AzureOpenAIApiVersion;

        #endregion
    }
}
