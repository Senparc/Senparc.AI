using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    /// <summary>
    /// AI platform type
    /// </summary>
    public enum AiPlatform
    {
        UnSet = 0,
        None = 1,
        Other = 2,
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
