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
        /*注意：值一旦确定，请勿修改！*/
        Other = -1,
        Unknown = 0,
        TextCompletion = 1,
        Chat = 2,
        TextEmbedding = 3,
        TextToImage = 4,
        [Obsolete("请使用 " + nameof(TextToImage))]
        ImageGeneration = 4,
        ImageToText = 5,
        TextToSpeech = 6,
        SpeechToText = 7,
        SpeechRecognition = 8
    }
}
