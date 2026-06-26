<<<<<<< HEAD
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    /// <summary>
    /// AI platform type
    /// </summary>
=======
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    /// <summary>
    /// AI 平台类型
    /// </summary>
>>>>>>> origin/developer
    public enum AiPlatform
    {
        UnSet = 0,
        None = 1,
        Other = 2,
<<<<<<< HEAD
        //[Obsolete("expired, please use NeuCharAI", true)]
        //NeuCharOpenAI = 4,
        NeuCharAI = 4,
        OpenAI = 8,
        AzureOpenAI = 16,
        HuggingFace = 32,
        //Oobabooga = 64,//not implemented
        FastAPI = 128,
        Ollama = 256,
        DeepSeek = 512
    }

    /// <summary>
    /// Configure model type
    /// </summary>
    public enum ConfigModel
    {
        /*note:Do not modify values after they are determined!*/
        Other = -1,
        Unknown = 0,
        TextCompletion = 1,
        Chat = 2,
        TextEmbedding = 3,
        TextToImage = 4,
        [Obsolete("please use " + nameof(TextToImage))]
        ImageGeneration = 4,
        ImageToText = 5,
        TextToSpeech = 6,
        SpeechToText = 7,
        SpeechRecognition = 8
    }

#pragma warning disable CS1591 // missing XML comment for publicly visible type or member XML comment
    //public enum VectorDBType
    //{
    //    Memory,
    //    HardDisk,
    //    Redis,
    //    Mulivs,
    //    Chroma,
    //    PostgreSQL,
    //    Sqlite,
    //    SqlServer,
    //    Default = Memory,
    //}

#pragma warning restore CS1591 // missing XML comment for publicly visible type or member XML comment
}
=======
        //[Obsolete("已过期，请使用 NeuCharAI", true)]
        //NeuCharOpenAI = 4,
        NeuCharAI = 4,
        OpenAI = 8,
        AzureOpenAI = 16,
        HuggingFace = 32,
        //Oobabooga = 64,//未实现
        FastAPI = 128,
        Ollama = 256,
        DeepSeek = 512,
        Anthropic = 1024,
        Gemini = 2048,
        Qwen = 4096,
        Kimi = 8192,
        XunFei = 16384
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

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    //public enum VectorDBType
    //{
    //    Memory,
    //    HardDisk,
    //    Redis,
    //    Mulivs,
    //    Chroma,
    //    PostgreSQL,
    //    Sqlite,
    //    SqlServer,
    //    Default = Memory,
    //}

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}
>>>>>>> origin/developer
