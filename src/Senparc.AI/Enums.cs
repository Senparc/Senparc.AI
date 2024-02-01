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
        Other = 2,
        [Obsolete("已过期，请使用 NeuCharAI", true)]
        NeuCharOpenAI = 4,
        NeuCharAI = 4,
        OpenAI = 8,
        AzureOpenAI = 16,
        HuggingFace = 32,
        //Oobabooga = 64,//未实现
        FastAPI = 128
    }

    /// <summary>
    /// 配置模型类型
    /// </summary>
    public enum ConfigModel
    {
        Chat,
        TextCompletion,
        TextEmbedding,
        ImageGeneration,
    }
}
