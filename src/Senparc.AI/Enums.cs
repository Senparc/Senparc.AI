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
        //[Obsolete("Deprecated. Use NeuCharAI", true)]
        //NeuCharOpenAI = 4,
        NeuCharAI = 4,
        OpenAI = 8,
        AzureOpenAI = 16,
        HuggingFace = 32,
        //Oobabooga = 64,//Not implemented
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
    /// Configured model type
    /// </summary>
    public enum ConfigModel
    {
        /*Important: do not change values once assigned.*/
        Other = -1,
        Unknown = 0,
        TextCompletion = 1,
        Chat = 2,
        TextEmbedding = 3,
        TextToImage = 4,
        [Obsolete("Use " + nameof(TextToImage))]
        ImageGeneration = 4,
        ImageToText = 5,
        TextToSpeech = 6,
        SpeechToText = 7,
        SpeechRecognition = 8
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
