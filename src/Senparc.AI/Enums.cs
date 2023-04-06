using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    /// <summary>
    /// AI 平台类型
    /// </summary>
    public enum AiPlatform
    {
        UnSet = 0,
        None = 1,
        OpenAI = 2,
        AzureOpenAI = 4
    }

    /// <summary>
    /// 配置模型类型
    /// </summary>
    public enum ConfigModel
    {
        TextCompletion,
        Embedding,
    }
}
