/**
* Last Modified: 20231207 By FelixJ
* 修改了一些拼写错误
* 更新了部分方法的XML注释以符合代码
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
    /// Kernel 及模型设置的扩展类
    /// </summary>
    public static partial class KernelConfigExtension
    {

        #region 初始化

        public static IWantToConfig IWantTo(this AgentAiHandler handler, ISenparcAiSetting senparcAiSetting = null)
        {
            var iWantTo = new IWantToConfig(new IWantTo(handler, senparcAiSetting));
            return iWantTo;
        }

        #endregion

        #region 配置 Kernel 生成条件

        /// <summary>
        /// 配置模型
        /// </summary>
        /// <param name="iWantToConfig"></param>
        /// <param name="configModel"></param>
        /// <param name="userId"></param>
        /// <param name="modelName">模型名称配置，如果为 null，则从配置中自动获取</param>
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
                //case AI.ConfigModel.TextToImage:
                //    modelNameStr = modelName.TextToImage;
                //    kernelBuilder = iWantTo.SemanticKernelHelper.ConfigImageGeneration(userId, existedKernelBuilder, modelNameStr, senparcAiSetting, GetDeploymentName(modelNameStr));
                //    //Console.WriteLine($"[调试]GetDeploymentName：{modelNameStr} / {GetDeploymentName(modelNameStr)}");
                //    //Console.WriteLine($"[调试]{senparcAiSetting.AiPlatform}-{senparcAiSetting.AzureOpenAIKeys.DeploymentName}-{senparcAiSetting.AzureOpenAIKeys.AzureEndpoint}\r\n{senparcAiSetting.AzureOpenAIKeys.ModelName.ToJson(true)}");
                //    break;
                //case AI.ConfigModel.SpeechToText:
                //    modelNameStr = modelName.SpeechToText ?? "whisper"; // 默认使用 whisper
                //    kernelBuilder = iWantTo.SemanticKernelHelper.ConfigAudioToText(userId, existedKernelBuilder, modelNameStr, senparcAiSetting, GetDeploymentName(modelNameStr));
                //    break;
                default:
                    throw new SenparcAiException("未处理当前 ConfigModel 类型：" + configModel);
            }

            iWantTo.KernelBuilder = kernelBuilder; //进行 Config 必须提供 Kernel
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

        ///// <summary>
        ///// 添加 TextCompletion 配置
        ///// </summary>
        ///// <param name="iWantToConfig"></param>
        ///// <param name="modelName">如果为 null，则从先前配置中读取</param>
        ///// <returns></returns>
        ///// <exception cref="SenparcAiException"></exception>
        //public static IWantToConfig AddTextCompletion(this IWantToConfig iWantToConfig, string? modelName = null)
        //{
        //    var aiPlatForm = iWantToConfig.IWantTo.SemanticKernelHelper.AiSetting.AiPlatform;
        //    var kernel = iWantToConfig.IWantTo.Kernel;
        //    var skHelper = iWantToConfig.IWantTo.SemanticKernelHelper;
        //    var aiSetting = skHelper.AiSetting;
        //    var userId = iWantToConfig.IWantTo.UserId;
        //    modelName ??= iWantToConfig.IWantTo.ModelName;
        //    var serviceId = skHelper.GetServiceId(userId, modelName);

        //    //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

        //    kernel.Config.AddTextCompletionService(serviceId, k =>
        //        aiPlatForm switch
        //        {
        //            AiPlatform.OpenAI => new OpenAITextCompletion(modelName, aiSetting.ApiKey, aiSetting.OrganizationId),

        //            AiPlatform.AzureOpenAI => new AzureTextCompletion(modelName, aiSetting.AzureEndpoint, aiSetting.ApiKey, aiSetting.AzureOpenAIApiVersion),

        //            _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
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
            var setting = iWantToRun.SemanticKernelHelper.AiSetting;
            var vectorStore = iWantToRun.GetVectorStore(setting.VectorDB);
            var store = new TextSearchStore(iWantToRun, vectorStore);
            return store;
        }

        #endregion

        #region Build Kernel

        public static IWantToRun BuildKernel(this IWantToConfig iWantToConfig, ChatClientAgentOptions chatClientAgentOptions = null, Action<IAIKernelBuilder>? kernelBuilderAction = null)
        {
            var iWantTo = iWantToConfig.IWantTo;
            var handler = iWantTo.AgentKernelHelper;
            handler.BuildKernel(iWantTo.KernelBuilder, chatClientAgentOptions, kernelBuilderAction);

            return new IWantToRun(new IWantToBuild(iWantToConfig));
        }

        public static async Task<IWantToRun> BuildKernelAsync(this IWantToConfig iWantToConfig, ChatClientAgentOptions chatClientAgentOptions = null, bool createAgentSession = false, Action<IAIKernelBuilder>? kernelBuilderAction = null)
        {
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
        //    var handler = iWantTo.SemanticKernelHelper;
        //    handler.BuildKernel(iWantTo.KernelBuilder, kernelBuilderAction);

        //    return new IWantToRun(new IWantToBuild(iWantToConfig));
        //}

        #endregion

        #region 运行准备

        /// <summary>
        /// 创建请求实体
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="requestContent"></param>
        /// <param name="useAllRegisteredFunctions">是否使用所有已经注册、创建过的 Function</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string? requestContent, AgentSession agentSession, bool useAllRegisteredFunctions = false,
            params AIFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new AIFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, requestContent!, iWantToRun.PromptConfigParameter, agentSession, pipeline);
            return request;
        }

        /// <summary>
        /// 创建请求实体，使用上下文，不提供单独的 prompt
        /// </summary> 
        /// <param name="iWantToRun"></param>
        /// <param name="useAllRegisteredFunctions">是否使用所有已经注册、创建过的 Function</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, AgentSession agentSession = null, bool useAllRegisteredFunctions = false, params AIFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent: null, agentSession: agentSession, useAllRegisteredFunctions, pipeline);
        }

        /// <summary>
        /// 创建请求实体（不使用所有已经注册、创建过的 Function，也不储存行下文）
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
        /// 创建请求实体
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="arguments"></param>
        /// <param name="useAllRegisteredFunctions">是否使用所有已经注册、创建过的 Function</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, KernelArguments arguments, AgentSession agentSession = null,
            bool useAllRegisteredFunctions = false, params AIFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new AIFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, arguments, iWantToRun.PromptConfigParameter, agentSession, pipeline);
            return request;
        }

        /// <summary>
        /// 创建请求实体（不使用所有已经注册、创建过的 Function，也不储存行下文）
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="contextVariables"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, KernelArguments contextVariables, AgentSession agentSession, params AIFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, contextVariables, agentSession, false, pipeline);
        }

        #endregion


    }
}