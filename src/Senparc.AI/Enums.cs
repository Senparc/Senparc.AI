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
        NeuCharOpenAI = 4,
        AzureOpenAI = 8,
        HuggingFace = 16,
        //Oobabooga = 32
    }

    /// <summary>
    /// 配置模型类型
    /// </summary>
    public enum ConfigModel
    {
        TextCompletion,
        TextEmbedding,
        ImageGeneration,
    }
}
