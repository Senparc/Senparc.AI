/**
* Last Modified: 20231207 By FelixJ
* Fixed several spelling errors
* Updated some XML comments to match the code
*/


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Connectors.Redis;
using OllamaSharp.Models;
using Qdrant.Client;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Helpers;
using Senparc.CO2NET.Extensions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Senparc.AI.Interfaces.VectorDB;

namespace Senparc.AI.Kernel.Handlers
{
    /// <summary>
    /// Extension class for Kernel and model settings
    /// </summary>
    public static partial class KernelConfigExtension
    {
        #region Initialize

        public static IWantToConfig IWantTo(this SemanticAiHandler handler, ISenparcAiSetting senparcAiSetting = null)
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

            IKernelBuilder kernelBuilder;

            switch (configModel)
            {
                case AI.ConfigModel.Chat:
                    modelNameStr = modelName.Chat;
                    kernelBuilder = iWantTo.SemanticKernelHelper.ConfigChat(userId, modelNameStr, senparcAiSetting,
                    existedKernelBuilder, GetDeploymentName(modelNameStr));
                    break;
                case AI.ConfigModel.TextCompletion:
                    modelNameStr = modelName.TextCompletion;
                    kernelBuilder = iWantTo.SemanticKernelHelper.ConfigTextCompletion(userId, modelNameStr, senparcAiSetting,
                    existedKernelBuilder, GetDeploymentName(modelNameStr));
                    break;
                case AI.ConfigModel.TextEmbedding:
                    modelNameStr = modelName.Embedding;
                    kernelBuilder = iWantTo.SemanticKernelHelper.ConfigTextEmbeddingGeneration(userId, modelNameStr, senparcAiSetting, existedKernelBuilder, GetDeploymentName(modelNameStr));
                    break;
                case AI.ConfigModel.TextToImage:
                    modelNameStr = modelName.TextToImage;
                    kernelBuilder = iWantTo.SemanticKernelHelper.ConfigImageGeneration(userId, existedKernelBuilder, modelNameStr, senparcAiSetting, GetDeploymentName(modelNameStr));
                    //Console.WriteLine($"[Debug]GetDeploymentName:{modelNameStr} / {GetDeploymentName(modelNameStr)}");
                    //Console.WriteLine($"[Debug]{senparcAiSetting.AiPlatform}-{senparcAiSetting.AzureOpenAIKeys.DeploymentName}-{senparcAiSetting.AzureOpenAIKeys.AzureEndpoint}\r\n{senparcAiSetting.AzureOpenAIKeys.ModelName.ToJson(true)}");
                    break;
                case AI.ConfigModel.SpeechToText:
                    modelNameStr = modelName.SpeechToText ?? "whisper"; // Use whisper by default
                    kernelBuilder = iWantTo.SemanticKernelHelper.ConfigAudioToText(userId, existedKernelBuilder, modelNameStr, senparcAiSetting, GetDeploymentName(modelNameStr));
                    break;
                case AI.ConfigModel.TextToSpeech:
                    modelNameStr = modelName.TextToSpeech ?? "tts"; // Use tts by default
                    kernelBuilder = iWantTo.SemanticKernelHelper.ConfigTextToAudio(userId, existedKernelBuilder, modelNameStr, senparcAiSetting, GetDeploymentName(modelNameStr));
                    break;
                default:
                    throw new SenparcAiException("Unhandled ConfigModel type: " + configModel);
            }

            iWantTo.KernelBuilder = kernelBuilder; //Kernel is required before Config
            iWantTo.UserId = userId;
            iWantTo.ModelName = modelNameStr;
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
        //    var aiPlatForm = iWantToConfig.IWantTo.SemanticKernelHelper.AiSetting.AiPlatform;
        //    var kernel = iWantToConfig.IWantTo.Kernel;
        //    var skHelper = iWantToConfig.IWantTo.SemanticKernelHelper;
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
        /// Config Vector Database
        /// </summary>
        /// <param name="iWantToConfig"></param>
        /// <param name="vectorDb"></param>
        /// <returns></returns>
        public static IWantToConfig ConfigVectorStore(this IWantToConfig iWantToConfig, VectorDB vectorDb
        /*ISenparcAiSetting? senparcAiSetting = null*/)
        {

            var kb = iWantToConfig.SemanticKernelHelper.KernelBuilder;

            var servives = kb.Services;

            switch (vectorDb.Type)
            {
                case VectorDBType.Memory:
                    {
                        servives.AddInMemoryVectorStore();
                        break;
                    }
                case VectorDBType.HardDisk:
                    {
                        servives.AddInMemoryVectorStore();
                        break;
                    }
                //case VectorDBType.Qdrant:
                //    {
                //        servives.AddQdrantVectorStore(vectorDb.ConnectionString);
                //        break;
                //    }
                case VectorDBType.Redis:
                    {
                        servives.AddRedisVectorStore(vectorDb.ConnectionString);
                        break;
                    }
                case VectorDBType.Milvus:
                    {
                        // servives.AddInMemoryVectorStore();
                        break;
                    }
                case VectorDBType.Chroma:
                    {
                        // servives.AddInMemoryVectorStore();
                        break;
                    }
                case VectorDBType.PostgreSQL:
                    {
                        // servives.AddInMemoryVectorStore();
                        break;
                    }
                case VectorDBType.Sqlite:
                    {
                        // servives.AddInMemoryVectorStore();
                        break;
                    }
                case VectorDBType.SqlServer:
                    {
                        break;
                    }
                case VectorDBType.Qdrant:
                    {
                        servives.AddQdrantVectorStore(vectorDb.ConnectionString);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(vectorDb.Type), $"Unsupported VectorDB type: {vectorDb.Type}");
                    }
            }

            return iWantToConfig;
        }

        /// <summary>
        /// Get Vector Collection
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="iWwantToRun"></param>
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
        public static VectorStoreCollection<TKey, TRecord> GetVectorCollection<TKey, TRecord>(this IWantToRun iWwantToRun, VectorDB vectorDb, string name, VectorStoreCollectionDefinition? vectorStoreRecordDefinition = null)
             where TKey : notnull
             where TRecord : class
        {
            IDatabase database;
            VectorStore vectorStore;
            VectorStoreCollection<TKey, TRecord> collection = null;

            //TODO: If the logic becomes overly complex in the future, different combinations can be considered to be separated into different libraries

            switch (vectorDb.Type)
            {
                case VectorDBType.Memory:
                    {
                        vectorStore = new InMemoryVectorStore();
                        collection = vectorStore.GetCollection<TKey, TRecord>(name, vectorStoreRecordDefinition);
                        break;
                    }
                case VectorDBType.HardDisk:
                    {
                        break;
                    }
                case VectorDBType.Redis:
                    {
                        database = ConnectionMultiplexer.Connect(vectorDb.ConnectionString).GetDatabase();
                        vectorStore = new RedisVectorStore(database,
                            new() { StorageType = RedisStorageType.Json });
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
                        vectorStore = new QdrantVectorStore(new QdrantClient(vectorDb.ConnectionString), ownsClient: true);
                        collection = vectorStore.GetCollection<TKey, TRecord>(name, vectorStoreRecordDefinition);
                        break;
                    }
                default:
                    vectorStore = new InMemoryVectorStore();
                    collection = vectorStore.GetCollection<TKey, TRecord>(name, vectorStoreRecordDefinition);
                    break;
            }

            return collection;
        }


        #endregion

        #region Build Kernel

        public static IWantToRun BuildKernel(this IWantToConfig iWantToConfig, Action<IKernelBuilder>? kernelBuilderAction = null)
        {
            var iWantTo = iWantToConfig.IWantTo;
            var handler = iWantTo.SemanticKernelHelper;
            handler.BuildKernel(iWantTo.KernelBuilder, kernelBuilderAction);

            return new IWantToRun(new IWantToBuild(iWantToConfig));
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

        #region Run preparation

        /// <summary>
        /// Create a request entity
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="requestContent"></param>
        /// <param name="useAllRegisteredFunctions">Whether to use all registered or created Functions</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string? requestContent, bool useAllRegisteredFunctions = false,
            params KernelFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //Merge registered objects
                pipeline = iWantToRun.Functions.Union(pipeline ?? new KernelFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, requestContent!, iWantToRun.PromptConfigParameter,
                pipeline);
            return request;
        }

        /// <summary>
        /// Create a request entity with context and without a separate prompt
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="useAllRegisteredFunctions">Whether to use all registered or created Functions</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, bool useAllRegisteredFunctions = false, params KernelFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent: null, useAllRegisteredFunctions, pipeline);
        }

        /// <summary>
        /// Create a request entity without all registered or created Functions and without storing context
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="requestContent"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string requestContent, params KernelFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent, false, pipeline);
        }

        /// <summary>
        /// Create a request entity
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="arguments"></param>
        /// <param name="useAllRegisteredFunctions">Whether to use all registered or created Functions</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, KernelArguments arguments,
            bool useAllRegisteredFunctions = false, params KernelFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //Merge registered objects
                pipeline = iWantToRun.Functions.Union(pipeline ?? new KernelFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, arguments, iWantToRun.PromptConfigParameter,
                pipeline);
            return request;
        }

        /// <summary>
        /// Create a request entity without all registered or created Functions and without storing context
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="contextVariables"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, KernelArguments contextVariables, params KernelFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, contextVariables, false, pipeline);
        }

        #endregion

        #region Run stage or supplemental settings after Kernel generation

        #region Context management

        /// <summary>
        /// Set context
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SenparcAiRequest SetTempContext(this SenparcAiRequest request, string key, string value)
        {
            request.TempAiArguments ??= new SenparcAiArguments();
            request.TempAiArguments.KernelArguments.Set(key, value);
            return request;
        }

        /// <summary>
        /// Set context
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SenparcAiRequest SetStoredContext(this SenparcAiRequest request, string key, object value)
        {
            request.StoreAiArguments.KernelArguments.Set(key, value);
            return request;
        }


        /// <summary>
        /// Get a context value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetTempArguments(this SenparcAiRequest request, string key, out object? value)
        {
            return request.TempAiArguments.KernelArguments.TryGetValue(key, out value);
        }

        /// <summary>
        /// Get a context value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetStoredArguments(this SenparcAiRequest request, string key, out object? value)
        {
            return request.StoreAiArguments.KernelArguments.TryGetValue(key, out value);
        }

        #endregion

        #region run

        /// <summary>
        /// run
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunAsync(this IWantToRun iWanToRun, SenparcAiRequest request, Action<StreamingKernelContent> inStreamItemProceessing = null)
        {
            return RunAsync<string>(iWanToRun, request, inStreamItemProceessing);
        }

        /// <summary>
        /// run
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        /// <typeparam name="T">Specify the return result type</typeparam>
        /// <returns></returns>

        public static async Task<SenparcKernelAiResult<T>> RunAsync<T>(this IWantToRun iWanToRun, SenparcAiRequest request, Action<StreamingKernelContent> inStreamItemProceessing = null)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            var prompt = request.RequestContent;
            var functionPipline = request.FunctionPipeline;
            //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

            //Note: Context is required whenever Plugin and Function are used and an input identifier is included

            iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
            var storedArguments = iWanToRun.StoredAiArguments.KernelArguments;
            var tempArguments = request.TempAiArguments?.KernelArguments;

            FunctionResult? functionResult = null;
            var result = new SenparcKernelAiResult<T>(iWanToRun, inputContent: null);

            var useStream = inStreamItemProceessing != null;

            if (tempArguments != null && tempArguments.Count() != 0)
            {
                //Input the temporary context for this request
                if (useStream)
                {
                    result.StreamResult = kernel.InvokeStreamingAsync(functionPipline.FirstOrDefault(), tempArguments);
                }
                else
                {
                    functionResult = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), tempArguments);
                }
                result.InputContext = new SenparcAiArguments(tempArguments);
            }
            else if (!prompt.IsNullOrEmpty())
            {
                //tempArguments is empty
                //Input plain text
                if (functionPipline?.Length > 0)
                {
                    //Use Pipeline
                    tempArguments = new() { ["input"] = prompt };

                    if (useStream)
                    {
                        result.StreamResult = kernel.InvokeStreamingAsync(functionPipline.First(), tempArguments);
                    }
                    else
                    {
                        //TODO: This method does not send body content to the server in the NeuCharAI interface
                        functionResult = await kernel.InvokeAsync(functionPipline.First(), tempArguments);
                    }
                }
                else
                {
                    //Do not use Pipeline

                    //note:Even if prompt is passed directly as the first string parameter here, it is wrapped into Context,
                    //      and assigned to the parameter whose key is INPUT
                    //var kernelFunction = iWanToRun.CreateFunctionFromPrompt(prompt ?? "").function;

                    if (useStream)
                    {
                        result.StreamResult = kernel.InvokePromptStreamingAsync(prompt ?? "", storedArguments);
                    }
                    else
                    {
                        functionResult = await kernel.InvokePromptAsync(prompt ?? "", storedArguments);
                    }
                }

                result.InputContent = prompt;
            }
            else
            {
                //Input context from cache
                //botAnswer = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), storedArguments);

                if (useStream)
                {
                    result.StreamResult = kernel.InvokeStreamingAsync(functionPipline.FirstOrDefault(), storedArguments);
                }
                else
                {
                    functionResult = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), storedArguments);
                }
                result.InputContext = new SenparcAiArguments(storedArguments);
            }

            result.InputContent = prompt;

            if (!useStream)
            {
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        result.OutputString = functionResult.GetValue<string>()?.TrimStart('\n') ?? "";
                    }
                    else
                    {
                        result.OutputString = functionResult.GetValue<T>()?.ToJson()?.TrimStart('\n') ?? "";
                    }
                }
                catch (Exception)
                {
                    //TODO: Provide generic Output support
                    result.OutputString = functionResult.GetValue<object>()?.ToJson()?.TrimStart('\n') ?? "";
                    _ = new SenparcAiException("Cannot convert to the specified type: " + typeof(T).Name);
                }
                result.Result = functionResult;
            }
            else
            {
                var stringResult = new StringBuilder();

                if (result.StreamResult != null)
                {
                    await foreach (var item in result.StreamResult)
                    {
                        stringResult.Append(item);
                        inStreamItemProceessing?.Invoke(item);//Execute stream callback
                    }
                }

                result.OutputString = stringResult.ToString();
            }

            //result.LastException = botAnswer.LastException;

            return result;
        }

        /// <summary>
        /// Run with Stream
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item.</param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunStreamAsync(this IWantToRun iWanToRun, SenparcAiRequest request, Action<StreamingKernelContent> inStreamItemProceessing = null)
        {
            inStreamItemProceessing ??= (item) => { };
            return RunAsync(iWanToRun, request, inStreamItemProceessing);
        }

        /// <summary>
        /// run
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iWanToRun"></param>
        /// <param name="kernelFunction"></param>
        /// <returns></returns>
        public static async Task<SenparcKernelAiResult> RunAsync(this IWantToRun iWanToRun, KernelFunction kernelFunction)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            var result = new SenparcKernelAiResult(iWanToRun, inputContent: null);

            var kernelResult = await kernel.InvokeAsync(kernelFunction);

            try
            {
                result.OutputString = kernelResult.GetValue<string>()?.TrimStart('\n') ?? "";
            }
            catch (Exception)
            {
                //TODO: Provide generic Output support
                result.OutputString = kernelResult.GetValue<object>()?.ToJson()?.TrimStart('\n') ?? "";
            }

            result.Result = kernelResult;

            return result;
        }

        /// <summary>
        /// run
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iWanToRun"></param>
        /// <param name="kernelFunction"></param>
        /// <returns></returns>
        public static async Task<SenparcAiResult<T>> RunAsync<T>(this IWantToRun iWanToRun, KernelFunction kernelFunction)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            var result = new SenparcAiResult<T>(iWanToRun, inputContent: null);

            var kernelResult = await kernel.InvokeAsync(kernelFunction);

            try
            {
                if (typeof(T) == typeof(string))
                {
                    result.OutputString = kernelResult.GetValue<string>()?.TrimStart('\n') ?? "";
                }
                else
                {
                    result.OutputString = kernelResult.GetValue<T>()?.ToJson()?.TrimStart('\n') ?? "";
                }
            }
            catch (Exception)
            {
                //TODO: Provide generic Output support
                result.OutputString = kernelResult.GetValue<object>()?.ToJson()?.TrimStart('\n') ?? "";
                _ = new SenparcAiException("Cannot convert to the specified type: " + typeof(T).Name);
            }

            result.Result = kernelResult.GetValue<T>();
            //result.LastException = botAnswer.LastException;

            return result;
        }

        #endregion

        #region Vision model run

        /// <summary>
        /// Run Vision model
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunVisionAsync(this IWantToRun iWanToRun,
            SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
            Action<StreamingKernelContent> inStreamItemProceessing = null)
        {
            return RunVisionAsync<string>(iWanToRun, request, chatHistory, contentList, inStreamItemProceessing);
        }

        /// <summary>
        /// Run Vision model
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        /// <typeparam name="T">Specify the return result type</typeparam>
        /// <returns></returns>

        public static async Task<SenparcKernelAiResult<T>> RunVisionAsync<T>(this IWantToRun iWanToRun,
            SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
            Action<StreamingKernelContent> inStreamItemProceessing = null)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;

            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            var prompt = request.RequestContent;
            var functionPipline = request.FunctionPipeline;
            //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

            //Note: Context is required whenever Plugin and Function are used and an input identifier is included

            iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
            var storedArguments = iWanToRun.StoredAiArguments.KernelArguments;
            var tempArguments = request.TempAiArguments?.KernelArguments;

            FunctionResult? functionResult = null;
            var result = new SenparcKernelAiResult<T>(iWanToRun, inputContent: null);

            var useStream = inStreamItemProceessing != null;

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            ChatMessageContentItemCollection contentItems = new ChatMessageContentItemCollection();
            foreach (var contentItem in contentList)
            {
                //if (contentItem.Type == Helpers.ContentType.Text)
                //{
                //    contentItems.Add(new TextContent(contentItem.TextContent));
                //}
                //else if (contentItem.Type == Helpers.ContentType.Image)
                //{
                //    contentItems.Add(new ImageContent_ImageBase64(contentItem.ImageData, "image/jpg"));
                //}
                if (contentItem is ContentItem_Text ciText)
                {
                    contentItems.Add(new TextContent(ciText.TextContent));
                }
                else if (contentItem is ContentItem_ImageBse64 ciBae64)
                {
                    contentItems.Add(new ImageContent(ciBae64.ImageData, "image/jpg"));
                }
                else if (contentItem is ContentItem_ImageUrl ciImageUrl)
                {
                    contentItems.Add(new ImageContent("data:image/jpeg;base64," + ciImageUrl.image_url.Url));
                }
            }

            chatHistory.AddUserMessage(contentItems);

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 3500,
                Temperature = 0.7,
                TopP = 0.5,
            };
            PromptExecutionSettings? executionSettings = helper.GetExecutionSetting(parameter, helper.AiSetting);

            if (useStream)
            {
                result.StreamResult = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings: executionSettings, kernel: iWanToRun.Kernel);

                var stringResult = new StringBuilder();

                if (result.StreamResult != null)
                {
                    await foreach (var item in result.StreamResult)
                    {
                        stringResult.Append(item);
                        inStreamItemProceessing?.Invoke(item);//Execute stream callback
                    }
                }

                result.OutputString = stringResult.ToString();
            }
            else
            {
                var contentResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings: executionSettings, kernel: iWanToRun.Kernel);
                //result.Result = contentResult;
                result.OutputString = contentResult.ToString();
            }

            return result;
        }

        #region Chat


        /// <summary>
        /// Run Chat + Vision model
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunChatVisionAsync(this IWantToRun iWanToRun,
            SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
            PromptConfigParameter? parameter = null,
            Action<StreamingKernelContent> inStreamItemProceessing = null)
        {
            return RunChatVisionAsync<string>(iWanToRun, request, chatHistory, contentList, parameter, inStreamItemProceessing);
        }

        /// <summary>
        /// Run Chat + Vision model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="chatHistory"></param>
        /// <param name="contentList"></param>
        /// <param name="parameter"></param>
        /// <param name="inStreamItemProceessing"></param>
        /// <returns></returns>
        public static async Task<SenparcKernelAiResult<T>> RunChatVisionAsync<T>(this IWantToRun iWanToRun,
           SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
            PromptConfigParameter? parameter = null,
           Action<StreamingKernelContent> inStreamItemProceessing = null)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;

            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            //Note: Context is required whenever Plugin and Function are used and an input identifier is included

            iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
            var storedArguments = iWanToRun.StoredAiArguments.KernelArguments;
            var tempArguments = request.TempAiArguments?.KernelArguments;

            FunctionResult? functionResult = null;
            var result = new SenparcKernelAiResult<T>(iWanToRun, inputContent: null);

            var useStream = inStreamItemProceessing != null;

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            ChatMessageContentItemCollection contentItems = new ChatMessageContentItemCollection();
            foreach (var contentItem in contentList)
            {
                //if (contentItem.Type == Helpers.ContentType.Text)
                //{
                //    contentItems.Add(new TextContent(contentItem.TextContent));
                //}
                //else if (contentItem.Type == Helpers.ContentType.Image)
                //{
                //    contentItems.Add(new ImageContent_ImageBase64(contentItem.ImageData, "image/jpg"));
                //}
                if (contentItem is ContentItem_Text ciText)
                {
                    contentItems.Add(new TextContent(ciText.TextContent));
                }
                else if (contentItem is ContentItem_ImageBse64 ciBae64)
                {
                    contentItems.Add(new ImageContent(ciBae64.ImageData, "image/jpg"));
                }
                else if (contentItem is ContentItem_ImageUrl ciImageUrl)
                {
                    contentItems.Add(new ImageContent("data:image/jpeg;base64," + ciImageUrl.image_url.Url));
                }
            }

            chatHistory.AddUserMessage(contentItems);

            parameter ??= new PromptConfigParameter()
            {
                MaxTokens = 3500,
                Temperature = 0.7,
                TopP = 0.5,
            };
            PromptExecutionSettings? executionSettings = helper.GetExecutionSetting(parameter, helper.AiSetting);

            if (kernel.Plugins.Count > 0)
            {
                executionSettings.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
            }

            if (useStream)
            {
                result.StreamResult = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings: executionSettings, kernel: iWanToRun.Kernel);

                var stringResult = new StringBuilder();

                if (result.StreamResult != null)
                {
                    await foreach (var item in result.StreamResult)
                    {
                        stringResult.Append(item);
                        inStreamItemProceessing?.Invoke(item);//Execute stream callback
                    }
                }

                result.OutputString = stringResult.ToString();
            }
            else
            {
                var contentResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings: executionSettings, kernel: iWanToRun.Kernel);
                //result.Result = contentResult;
                result.OutputString = contentResult.ToString();
            }

            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}