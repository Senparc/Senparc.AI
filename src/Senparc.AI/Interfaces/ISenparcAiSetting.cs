using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Exceptions;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// Senparc.AI base configuration with Items for multi-model configuration
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
    /// Senparc.AI base configuration
    /// </summary>
    public interface ISenparcAiSetting
    {
        /// <summary>
        /// Whether debug mode is enabled
        /// </summary>
        bool IsDebug { get; set; }

        VectorDB VectorDB { get; set; }

        /// <summary>
        /// MCP Server configuration collection.
        /// </summary>
        List<McpServerOption> McpServers { get; set; }

        string Endpoint => AiPlatform switch
        {
            AiPlatform.OpenAI => OpenAIEndpoint,
            AiPlatform.AzureOpenAI => AzureEndpoint,
            AiPlatform.NeuCharAI => NeuCharEndpoint,
            AiPlatform.HuggingFace => HuggingFaceEndpoint,
            AiPlatform.FastAPI => FastAPIEndpoint,
            AiPlatform.Ollama => OllamaEndpoint,
            AiPlatform.DeepSeek => DeepSeekEndpoint,
            AiPlatform.Anthropic => AnthropicEndpoint,
            AiPlatform.Gemini => GeminiEndpoint,
            AiPlatform.Qwen => QwenEndpoint,
            AiPlatform.Kimi => KimiEndpoint,
            AiPlatform.XunFei => XunFeiEndpoint,
            _ => throw new SenparcAiException($"Endpoint output is not configured for {AiPlatform}")
        };

        /// <summary>
        /// Whether Azure OpenAI is used
        /// </summary>
        bool UseAzureOpenAI => AiPlatform == AiPlatform.AzureOpenAI;

        /// <summary>
        /// Whether NeuChar OpenAI is used
        /// </summary>
        bool UseNeuCharAI => AiPlatform == AiPlatform.NeuCharAI;
        /// <summary>
        /// AI platform type
        /// </summary>
        AiPlatform AiPlatform { get; set; }

        AzureOpenAIKeys AzureOpenAIKeys { get; set; }
        NeuCharAIKeys NeuCharAIKeys { get; set; }
        OpenAIKeys OpenAIKeys { get; set; }
        HuggingFaceKeys HuggingFaceKeys { get; set; }
        FastAPIKeys FastAPIKeys { get; set; }
        OllamaKeys OllamaKeys { get; set; }
        DeepSeekKeys DeepSeekKeys { get; set; }
        AnthropicKeys AnthropicKeys { get; set; }
        GeminiKeys GeminiKeys { get; set; }
        QwenKeys QwenKeys { get; set; }
        KimiKeys KimiKeys { get; set; }
        XunFeiKeys XunFeiKeys { get; set; }

        /// <summary>
        /// NeuChar OpenAI, Azure OpenAI, or OpenAI API key
        /// </summary>
        string ApiKey { get; }

        /// <summary>
        /// OpenAI API Organization ID
        /// </summary>
        string OrganizationId { get; }

        /// <summary>
        /// Anthropic API version header value.
        /// </summary>
        string AnthropicVersion { get; }

        #region OpenAI

        /// <summary>
        /// OpenAI Endpoint
        /// </summary>
        string OpenAIEndpoint { get; }

        #endregion

        #region Azure OpenAI

        /// <summary>
        /// Azure OpenAI Endpoint
        /// </summary>
        string AzureEndpoint { get; }
        /// <summary>
        /// Azure OpenAI version
        /// </summary>
        string AzureOpenAIApiVersion { get; }

        #endregion

        #region NeuChar

        /// <summary>
        /// NeuChar OpenAI Endpoint
        /// </summary>
        string NeuCharEndpoint { get; }
        /// <summary>
        /// Azure OpenAI version
        /// </summary>
        //[Obsolete("Deprecated. Use NeuCharAIApiVersion", true)]
        //string NeuCharOpenAIApiVersion { get; }

        /// <summary>
        /// Corresponding Azure OpenAI version
        /// </summary>
        string NeuCharAIApiVersion { get; }

        #endregion

        #region HuggingFace

        string HuggingFaceEndpoint { get; }

        #endregion

        #region FastAPI

        string FastAPIEndpoint { get; }

        #endregion

        #region Ollama

        string OllamaEndpoint { get; }

        #endregion

        #region DeepSeek

        string DeepSeekEndpoint { get; }

        #endregion

        #region Anthropic

        string AnthropicEndpoint { get; }

        #endregion

        #region Gemini

        string GeminiEndpoint { get; }

        #endregion

        #region Qwen

        string QwenEndpoint { get; }

        #endregion

        #region Kimi

        string KimiEndpoint { get; }

        #endregion

        #region XunFei

        string XunFeiEndpoint { get; }

        #endregion

        /// <summary>
        /// Whether OpenAIKeys has been set
        /// </summary>
        public bool IsOpenAiKeysSetted { get; }


        public ModelName ModelName { get; }

#pragma warning disable CS8603 // Possible null reference return.
        public string DeploymentName { get; }
#pragma warning restore CS8603 // Possible null reference return.

    }


    //public enum VectorDBType
    //{
    //    //Memory,
    //    //HardDisk,
    //    //Redis,
    //    //Mulivs,
    //    //Chroma,
    //    //PostgreSQL,
    //    //Sqlite,
    //    //SqlServer,

    //    /* Important: do not change enum values once assigned */

    //    AzureAISearch=0,
    //    CosmosDBMongoDB=1,
    //    CosmosDBNoSQL=2,
    //    Chroma=3,     //Planed
    //    Couchbase=4,
    //    Elasticsearch=5,
    //    Faiss=6,
    //    InMemory=7,
    //    JDBC=8,
    //    Milvus=9,     //Planed (not included in https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/inmemory-connector?pivots=programming-language-csharp)
    //    MongoDB=10,
    //    Pinecon=11,
    //    Postgres=12,
    //    Qdrant=13,
    //    Redis=14,
    //    SqlServer=15,  //Planed
    //    SQLite=16,
    //    VolatileInMemory=17,
    //    Weaviate=18,
    //    Default = InMemory,
    //}


}
