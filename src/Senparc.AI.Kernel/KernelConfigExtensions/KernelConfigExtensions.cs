using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.HuggingFace.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Helpers;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Handlers
{
    /// <summary>
    /// Kernel 及模型设置的扩展类
    /// </summary>
    public static partial class KernelConfigExtension
    {
        #region 初始化

        public static IWantToConfig IWantTo(this SemanticAiHandler handler)
        {
            var iWantTo = new IWantToConfig(new IWantTo(handler));
            return iWantTo;
        }

        #endregion

        #region 配置 Kernel 生成条件

        public static IWantToConfig ConfigModel(this IWantToConfig iWantToConfig, ConfigModel configModel, string userId, string modelName,
            ISenparcAiSetting? senparcAiSetting = null)
        {
            var iWantTo = iWantToConfig.IWantTo;
            var existedKernelBuilder = iWantToConfig.IWantTo.KernelBuilder;
            var kernelBuilder = configModel switch
            {
                AI.ConfigModel.TextCompletion => iWantTo.SemanticKernelHelper.ConfigTextCompletion(userId, modelName, senparcAiSetting,
                    existedKernelBuilder),
                AI.ConfigModel.TextEmbedding => iWantTo.SemanticKernelHelper.ConfigTextEmbeddingGeneration(userId, modelName, existedKernelBuilder),
                AI.ConfigModel.ImageGeneration => iWantTo.SemanticKernelHelper.ConfigImageGeneration(userId, existedKernelBuilder),
                _ => throw new SenparcAiException("未处理当前 ConfigModel 类型：" + configModel)
            };
            iWantTo.KernelBuilder = kernelBuilder; //进行 Config 必须提供 Kernel
            iWantTo.UserId = userId;
            iWantTo.ModelName = modelName;
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

        #region Build Kernel

        public static IWantToRun BuildKernel(this IWantToConfig iWantToConfig, Action<KernelBuilder>? kernelBuilderAction = null)
        {
            var iWantTo = iWantToConfig.IWantTo;
            var handler = iWantTo.SemanticKernelHelper;
            handler.BuildKernel(iWantTo.KernelBuilder, kernelBuilderAction);

            return new IWantToRun(new IWantToBuild(iWantToConfig));
        }

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
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string? requestContent, bool useAllRegisteredFunctions = false,
            params ISKFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new ISKFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, iWantTo.ModelName, requestContent!, iWantToRun.PromptConfigParameter,
                pipeline);
            return request;
        }

        /// <summary>
        /// 创建请求实体，使用上下文，不提供单独的 prompt
        /// </summary> 
        /// <param name="iWantToRun"></param>
        /// <param name="useAllRegisteredFunctions">是否使用所有已经注册、创建过的 Function</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, bool useAllRegisteredFunctions = false, params ISKFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent: null, useAllRegisteredFunctions, pipeline);
        }

        /// <summary>
        /// 创建请求实体（不使用所有已经注册、创建过的 Function，也不储存行下文）
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="requestContent"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string requestContent, params ISKFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent, false, pipeline);
        }

        /// <summary>
        /// 创建请求实体
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="contextVariables"></param>
        /// <param name="useAllRegisteredFunctions">是否使用所有已经注册、创建过的 Function</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, ContextVariables contextVariables,
            bool useAllRegisteredFunctions = false, params ISKFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new ISKFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, iWantTo.ModelName, contextVariables, iWantToRun.PromptConfigParameter,
                pipeline);
            return request;
        }

        /// <summary>
        /// 创建请求实体（不使用所有已经注册、创建过的 Function，也不储存行下文）
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="contextVariables"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, ContextVariables contextVariables, params ISKFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, contextVariables, false, pipeline);
        }

        #endregion

        #region 运行阶段，或对生成后的 Kernel 进行补充设置

        #region 对上下文的管理

        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SenparcAiRequest SetTempContext(this SenparcAiRequest request, string key, string value)
        {
            request.TempAiContext ??= new SenparcAiContext();
            request.TempAiContext.ContextVariables.Set(key, value);
            return request;
        }

        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SenparcAiRequest SetStoredContext(this SenparcAiRequest request, string key, string value)
        {
            request.StoreAiContext.ContextVariables.Set(key, value);
            return request;
        }


        /// <summary>
        /// 获取上下文的值
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetTempContext(this SenparcAiRequest request, string key, out string value)
        {
            return request.TempAiContext.ContextVariables.TryGetValue(key, out value);
        }

        /// <summary>
        /// 获取上下文的值
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetStoredContext(this SenparcAiRequest request, string key, out string value)
        {
            return request.StoreAiContext.ContextVariables.TryGetValue(key, out value);
        }

        #endregion

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<SenparcKernelAiResult> RunAsync(this IWantToRun iWanToRun, SenparcAiRequest request)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.ISKFunction;

            var prompt = request.RequestContent;
            var functionPipline = request.FunctionPipeline;
            //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

            //注意：只要使用了 Plugin 和 Function，并且包含输入标识，就需要使用上下文

            iWanToRun.StoredAiContext ??= new SenparcAiContext();
            var storedContext = iWanToRun.StoredAiContext.ContextVariables;
            var tempContext = request.TempAiContext?.ContextVariables;

            KernelResult? botAnswer;

            var result = new SenparcKernelAiResult(iWanToRun, inputContent: null);

            if (tempContext != null && tempContext.Count() != 0)
            {
                //输入特定的本次请求临时上下文
                botAnswer = await kernel.RunAsync(tempContext, functionPipline);
                result.InputContext = new SenparcAiContext(tempContext);
            }
            else if (!prompt.IsNullOrEmpty())
            {
                //输入纯文字
                if (functionPipline?.Length > 0)
                {
                    tempContext = new ContextVariables();
                    tempContext["INPUT"] = prompt;

                    botAnswer = await kernel.RunAsync(tempContext, functionPipline);
                }
                else
                {
                    //注意：此处即使直接输入 prompt 作为第一个 String 参数，也会被封装到 Context，
                    //      并赋值给 Key 为 INPUT 的参数
                    botAnswer = await kernel.RunAsync(prompt, functionPipline);
                }

                result.InputContent = prompt;
            }
            else
            {
                //输入缓存中的上下文
                botAnswer = await kernel.RunAsync(storedContext, functionPipline);
                result.InputContext = new SenparcAiContext(storedContext);
            }

            result.InputContent = prompt;
            result.Output = botAnswer.GetValue<string>()?.TrimStart('\n') ?? "";
            result.Result = botAnswer;
            //result.LastException = botAnswer.LastException;

            return result;
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iWanToRun"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static async Task<SenaprcAiResult<T>> RunAsync<T>(this IWantToRun iWanToRun, params ISKFunction[] pipeline)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.ISKFunction;

            var result = new SenaprcAiResult<T>(iWanToRun, inputContent: null);

            var kernelResult = await kernel.RunAsync(pipeline);

            try
            {
                result.Output = kernelResult.GetValue<string>() ?? "";
            }
            catch (Exception)
            {
                _ = new SenparcAiException("无法转换为指定类型：" + typeof(T).Name);
            }

            result.Result = kernelResult.GetValue<T>();
            //result.LastException = botAnswer.LastException;

            return result;
        }

        #endregion
    }
}