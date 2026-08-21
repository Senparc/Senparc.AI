/**
* Last Modified: 20231207 By FelixJ
* Fixed several spelling errors
* Updated some XML comments to match the code
*/


using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.SemanticKernel.Data;
using Qdrant.Client;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Qdrant.Client.Grpc.BinaryQuantizationQueryEncoding.Types;
using static Senparc.AI.Interfaces.VectorDB;

namespace Senparc.AI.AgentKernel.Handlers
{
    /// <summary>
    /// Extension class for Kernel and model settings
    /// </summary>
    public static partial class KernelConfigExtension
    {

        #region Initialize

        public static IWantToConfig IWantTo(this AgentAiHandler handler, ISenparcAiSetting senparcAiSetting = null)
        {
            var iWantTo = new IWantToConfig(new IWantTo(handler, senparcAiSetting));
            return iWantTo;
        }

        #endregion

        #region Configure Kernel creation conditions

        /// <summary>
        /// Configure model
        /// </summary>
        /// <param name="iWantToConfig"></param>
        /// <param name="configModel"></param>
        /// <param name="userId"></param>
        /// <param name="modelName">Model name configuration. If null, it is read automatically from configuration.</param>
        /// <param name="senparcAiSetting"></param>
        /// <param name="deploymentName"></param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public static IWantToConfig ConfigModel(this IWantToConfig iWantToConfig, ConfigModel configModel, string userId, ModelName modelName = null,
            ISenparcAiSetting? senparcAiSetting = null, string deploymentName = null)
        {
            var iWantTo = iWantToConfig.IWantTo;
            var existedKernelBuilder = iWantToConfig.IWantTo.KernelBuilder;
            senparcAiSetting ??= iWantTo.SenparcAiSetting;
            modelName ??= senparcAiSetting.ModelName;

            string modelNameStr;

            Func<string, string> GetDeploymentName = (modelNameStr) =>
            {
                if (!deploymentName.IsNullOrEmpty())
                {
                    return deploymentName;
                }
                else if (!senparcAiSetting.DeploymentName.IsNullOrEmpty())
                {
                    return senparcAiSetting.DeploymentName;
                }
                return modelNameStr;
            };

            IAIKernelBuilder kernelBuilder;

            switch (configModel)
            {
                case AI.ConfigModel.Chat:
                    modelNameStr = modelName.Chat;
                    kernelBuilder = iWantTo.AgentKernelHelper.ConfigChat(userId, modelNameStr, senparcAiSetting,
                    existedKernelBuilder, GetDeploymentName(modelNameStr));
                    break;
                case AI.ConfigModel.TextCompletion:
                    modelNameStr = modelName.TextCompletion;
                    kernelBuilder = iWantTo.AgentKernelHelper.ConfigChat(userId, modelNameStr, senparcAiSetting,
                    existedKernelBuilder, GetDeploymentName(modelNameStr));//TODO:Update or Merge
                    break;
                case AI.ConfigModel.TextEmbedding:
                    modelNameStr = modelName.Embedding;
                    kernelBuilder = iWantTo.AgentKernelHelper.ConfigTextEmbeddingGeneration(userId, modelNameStr, senparcAiSetting, existedKernelBuilder, GetDeploymentName(modelNameStr));
                    break;
                case AI.ConfigModel.TextToImage:
                    modelNameStr = modelName.TextToImage;
                    kernelBuilder = iWantTo.AgentKernelHelper.ConfigImageGeneration(userId, modelNameStr, senparcAiSetting, existedKernelBuilder, GetDeploymentName(modelNameStr));
                    break;
                case AI.ConfigModel.SpeechToText:
                    modelNameStr = modelName.SpeechToText ?? "whisper";
                    kernelBuilder = iWantTo.AgentKernelHelper.ConfigSpeechToText(userId, modelNameStr, senparcAiSetting, existedKernelBuilder, GetDeploymentName(modelNameStr));
                    break;
                case AI.ConfigModel.TextToSpeech:
                    modelNameStr = modelName.TextToSpeech ?? "tts";
                    kernelBuilder = iWantTo.AgentKernelHelper.ConfigTextToSpeech(userId, modelNameStr, senparcAiSetting, existedKernelBuilder, GetDeploymentName(modelNameStr));
                    break;
                default:
                    throw new SenparcAiException("Unhandled ConfigModel type: " + configModel);
            }

            iWantTo.KernelBuilder = kernelBuilder; //Kernel is required before Config
            iWantTo.UserId = userId;
            iWantTo.ModelName = modelNameStr;
            return iWantToConfig;
        }

        public static IWantToConfig ConfigTextEmbeddingModel(this IWantToConfig iWantToConfig, string userId, string collectionName, ModelName modelName = null,
           ISenparcAiSetting? senparcAiSetting = null, string deploymentName = null)
        {
            iWantToConfig.ConfigModel(AI.ConfigModel.TextEmbedding, userId, modelName, senparcAiSetting, deploymentName);

            var kernelBuilder = iWantToConfig.IWantTo.KernelBuilder;
            kernelBuilder.EmbeddingCollectionName = collectionName;

            return iWantToConfig;
        }

        public static IWantToConfig ConfigChatModel(this IWantToConfig iWantToConfig, string userId, ChatClientAgentOptions options, ModelName modelName = null,
           ISenparcAiSetting? senparcAiSetting = null, string deploymentName = null)
        {
            iWantToConfig.ConfigModel(AI.ConfigModel.Chat, userId, modelName, senparcAiSetting, deploymentName);
            iWantToConfig.ChatClientAgentOptions = options;

            return iWantToConfig;
        }

        public static IWantToConfig ConfigImageModel(this IWantToConfig iWantToConfig, string userId, ModelName modelName = null,
           ISenparcAiSetting? senparcAiSetting = null, string deploymentName = null)
        {
            iWantToConfig.ConfigModel(AI.ConfigModel.TextToImage, userId, modelName, senparcAiSetting, deploymentName);
            return iWantToConfig;
        }

        public static IWantToConfig ConfigSpeechToTextModel(this IWantToConfig iWantToConfig, string userId, ModelName modelName = null,
           ISenparcAiSetting? senparcAiSetting = null, string deploymentName = null)
        {
            iWantToConfig.ConfigModel(AI.ConfigModel.SpeechToText, userId, modelName, senparcAiSetting, deploymentName);
            return iWantToConfig;
        }

        public static IWantToConfig ConfigTextToSpeechModel(this IWantToConfig iWantToConfig, string userId, ModelName modelName = null,
           ISenparcAiSetting? senparcAiSetting = null, string deploymentName = null)
        {
            iWantToConfig.ConfigModel(AI.ConfigModel.TextToSpeech, userId, modelName, senparcAiSetting, deploymentName);
            return iWantToConfig;
        }

        ///// <summary>
        ///// Add TextCompletion configuration
        ///// </summary>
        ///// <param name="iWantToConfig"></param>
        ///// <param name="modelName">If null, read from the previous configuration</param>
        ///// <returns></returns>
        ///// <exception cref="SenparcAiException"></exception>
        //public static IWantToConfig AddTextCompletion(this IWantToConfig iWantToConfig, string? modelName = null)
        //{
        //    var aiPlatForm = iWantToConfig.IWantTo.AgentKernelHelper.AiSetting.AiPlatform;
        //    var kernel = iWantToConfig.IWantTo.Kernel;
        //    var skHelper = iWantToConfig.IWantTo.AgentKernelHelper;
        //    var aiSetting = skHelper.AiSetting;
        //    var userId = iWantToConfig.IWantTo.UserId;
        //    modelName ??= iWantToConfig.IWantTo.ModelName;
        //    var serviceId = skHelper.GetServiceId(userId, modelName);

        //    //TODO Need to check Kernel.TextCompletionServices.ContainsKey(serviceId). If it already exists, do not add it again.

        //    kernel.Config.AddTextCompletionService(serviceId, k =>
        //        aiPlatForm switch
        //        {
        //            AiPlatform.OpenAI => new OpenAITextCompletion(modelName, aiSetting.ApiKey, aiSetting.OrganizationId),

        //            AiPlatform.AzureOpenAI => new AzureTextCompletion(modelName, aiSetting.AzureEndpoint, aiSetting.ApiKey, aiSetting.AzureOpenAIApiVersion),

        //            _ => throw new SenparcAiException($"does not handle current {nameof(AiPlatform)} type:{aiPlatForm}")
        //        }
        //    );

        //    return iWantToConfig;
        //}

        #endregion

        #region Vector Database

        /// <summary>
        /// Get VectorStore
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="vectorDb"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static VectorStore GetVectorStore(this IWantToRun iWantToRun, VectorDB vectorDb)
        {
            var embeddingGenerator = iWantToRun.Kernel.EmbeddingGenerator;

            VectorStore vectorStore = vectorDb.Type switch
            {
                VectorDBType.Memory => new InMemoryVectorStore(new() { EmbeddingGenerator = embeddingGenerator }),
                VectorDBType.HardDisk => throw new Exception("HardDisk VectorStore is Not Supported"),
                VectorDBType.Redis => new RedisVectorStore(ConnectionMultiplexer.Connect(vectorDb.ConnectionString).GetDatabase(),
                            new() { StorageType = RedisStorageType.Json }),
                VectorDBType.Milvus => throw new Exception("Milvus VectorStore is Not Supported"),
                VectorDBType.Chroma => throw new Exception("Chroma VectorStore is Not Supported"),
                VectorDBType.PostgreSQL => throw new Exception("PostgreSQL VectorStore is Not Supported"),
                VectorDBType.Sqlite => throw new Exception("Sqlite VectorStore is Not Supported"),
                VectorDBType.SqlServer => throw new Exception("SqlServer VectorStore is Not Supported"),
                VectorDBType.Qdrant => new QdrantVectorStore(new QdrantClient(vectorDb.ConnectionString), ownsClient: true),
                _ => throw new ArgumentOutOfRangeException(nameof(vectorDb.Type), $"Unsupported VectorDB type: {vectorDb.Type}")
            };

            return vectorStore;
        }

        /// <summary>
        /// Get Vector Collection
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="iWantToRun"></param>
        /// <param name="vectorDb"></param>
        /// <param name="name"></param>
        /// <param name="vectorStoreRecordDefinition"></param>
        /// <returns></returns>
        /// <remarks>
        /// Note: VectorStore instances created in this method are not explicitly disposed.
        /// The VectorStoreCollection returned may maintain a reference to the VectorStore and require it to remain alive.
        /// For proper resource management, consider using dependency injection to manage VectorStore lifecycle
        /// or ensure the collection is disposed when no longer needed.
        /// </remarks>
        public static VectorStoreCollection<TKey, TRecord> GetVectorCollection<TKey, TRecord>(this IWantToRun iWantToRun, VectorDB vectorDb, string name, VectorStoreCollectionDefinition? vectorStoreRecordDefinition = null)
             where TKey : notnull
             where TRecord : class
        {
            IDatabase database;
            VectorStore vectorStore = iWantToRun.GetVectorStore(vectorDb);
            VectorStoreCollection<TKey, TRecord> collection = null;

            //TODO: If the logic becomes overly complex in the future, different combinations can be considered to be separated into different libraries

            switch (vectorDb.Type)
            {
                case VectorDBType.Memory:
                    {
                        collection = vectorStore.GetCollection<TKey, TRecord>(name, vectorStoreRecordDefinition);
                        break;
                    }
                case VectorDBType.HardDisk:
                    {
                        break;
                    }
                case VectorDBType.Redis:
                    {
                        collection = vectorStore.GetCollection<TKey, TRecord>(name, vectorStoreRecordDefinition);
                        break;
                    }
                case VectorDBType.Milvus:
                    {
                        break;
                    }
                case VectorDBType.Chroma:
                    {
                        break;
                    }
                case VectorDBType.PostgreSQL:
                    {
                        break;
                    }
                case VectorDBType.Sqlite:
                    {
                        break;
                    }
                case VectorDBType.SqlServer:
                    {
                        break;
                    }
                case VectorDBType.Qdrant:
                    {
                        collection = vectorStore.GetCollection<TKey, TRecord>(name, vectorStoreRecordDefinition);
                        break;
                    }
                default:
                    collection = vectorStore.GetCollection<TKey, TRecord>(name, vectorStoreRecordDefinition);
                    break;
            }

            return collection;
        }

        public static TextSearchStore CreateTextSearchStore(this IWantToRun iWantToRun)
        {
            var setting = iWantToRun.AgentKernelHelper.AiSetting;
            var vectorStore = iWantToRun.GetVectorStore(setting.VectorDB);
            var store = new TextSearchStore(iWantToRun, vectorStore);
            return store;
        }

        #endregion

        #region Build Kernel

        public static IWantToRun BuildKernel(this IWantToConfig iWantToConfig, ChatClientAgentOptions chatClientAgentOptions = null, Action<IAIKernelBuilder>? kernelBuilderAction = null)
        {
            chatClientAgentOptions ??= iWantToConfig.ChatClientAgentOptions;

            var iWantTo = iWantToConfig.IWantTo;
            var handler = iWantTo.AgentKernelHelper;
            handler.BuildKernel(iWantTo.KernelBuilder, chatClientAgentOptions, kernelBuilderAction);

            return new IWantToRun(new IWantToBuild(iWantToConfig));
        }

        public static async Task<IWantToRun> BuildKernelAsync(this IWantToConfig iWantToConfig, ChatClientAgentOptions chatClientAgentOptions = null, bool createAgentSession = false, Action<IAIKernelBuilder>? kernelBuilderAction = null)
        {
            chatClientAgentOptions ??= iWantToConfig.ChatClientAgentOptions;

            if (createAgentSession)
            {
                return await BuildKernelWithAgentSessionAsync(iWantToConfig, chatClientAgentOptions, kernelBuilderAction);
            }
            else
            {
                return BuildKernel(iWantToConfig, chatClientAgentOptions, kernelBuilderAction);
            }
        }

        public static async Task<IWantToRun> BuildKernelWithAgentSessionAsync(this IWantToConfig iWantToConfig, ChatClientAgentOptions chatClientAgentOptions = null, Action<IAIKernelBuilder>? kernelBuilderAction = null, AgentSession agentSession = null)
        {
            chatClientAgentOptions ??= iWantToConfig.ChatClientAgentOptions;
            var iWantToRun = BuildKernel(iWantToConfig, chatClientAgentOptions, kernelBuilderAction);

            if (iWantToRun.Kernel.ChatClientAgent != null)
            {
                agentSession ??= await iWantToRun.Kernel.ChatClientAgent.CreateSessionAsync();
                await iWantToRun.Kernel.SetAgentSessionAsync(agentSession);
            }

            return iWantToRun;
        }

        //#pragma warning disable SKEXP0052
        //public static IWantToRun BuildMemoryKernel(this IWantToConfig iWantToConfig, Action<MemoryBuilder>? kernelBuilderAction = null)
        //{
        //    var iWantTo = iWantToConfig.IWantTo;
        //    var handler = iWantTo.AgentKernelHelper;
        //    handler.BuildKernel(iWantTo.KernelBuilder, kernelBuilderAction);

        //    return new IWantToRun(new IWantToBuild(iWantToConfig));
        //}

        #endregion

        #region Run preparation

        /// <summary>
        /// Create a request entity
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="requestContent"></param>
        /// <param name="useAllRegisteredFunctions">Whether to use all registered or created Functions</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string? requestContent, AgentSession agentSession, bool useAllRegisteredFunctions = false,
            params AIFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //Merge registered objects
                pipeline = iWantToRun.Functions.Union(pipeline ?? new AIFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, requestContent!, iWantToRun.PromptConfigParameter, agentSession, pipeline);
            return request;
        }

        /// <summary>
        /// Create a request entity with context and without a separate prompt
        /// </summary> 
        /// <param name="iWantToRun"></param>
        /// <param name="useAllRegisteredFunctions">Whether to use all registered or created Functions</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, AgentSession agentSession = null, bool useAllRegisteredFunctions = false, params AIFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent: null, agentSession: agentSession, useAllRegisteredFunctions, pipeline);
        }

        /// <summary>
        /// Create a request entity without all registered or created Functions and without storing context
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="requestContent"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string requestContent, AgentSession agentSession, params AIFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent, agentSession, false, pipeline);
        }

        /// <summary>
        /// Create a request entity
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="arguments"></param>
        /// <param name="useAllRegisteredFunctions">Whether to use all registered or created Functions</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, AgentKernelArguments arguments, AgentSession agentSession = null,
            bool useAllRegisteredFunctions = false, params AIFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //Merge registered objects
                pipeline = iWantToRun.Functions.Union(pipeline ?? new AIFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, arguments, iWantToRun.PromptConfigParameter, agentSession, pipeline);
            return request;
        }

        /// <summary>
        /// Create a request entity without all registered or created Functions and without storing context
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="contextVariables"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, AgentKernelArguments contextVariables, AgentSession agentSession, params AIFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, contextVariables, agentSession, false, pipeline);
        }

        #endregion


    }
}
