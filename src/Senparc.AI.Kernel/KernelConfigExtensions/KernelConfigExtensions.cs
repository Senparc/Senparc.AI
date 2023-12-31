/**
* Last Modified: 20231207 By FelixJ
* 修改了一些拼写错误
* 更新了部分方法的XML注释以符合代码
*/


using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Memory;
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
            ISenparcAiSetting? senparcAiSetting = null, string azureDallEDepploymentName = null)
        {
            var iWantTo = iWantToConfig.IWantTo;
            var existedKernelBuilder = iWantToConfig.IWantTo.KernelBuilder;
            var kernelBuilder = configModel switch
            {
                AI.ConfigModel.TextCompletion => iWantTo.SemanticKernelHelper.ConfigTextCompletion(userId, modelName, senparcAiSetting,
                    existedKernelBuilder, modelName),
                AI.ConfigModel.TextEmbedding => iWantTo.SemanticKernelHelper.ConfigTextEmbeddingGeneration(userId, modelName, existedKernelBuilder),
                AI.ConfigModel.ImageGeneration => iWantTo.SemanticKernelHelper.ConfigImageGeneration(userId, existedKernelBuilder, modelName, azureDallEDepploymentName),
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
            params KernelFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new KernelFunction[0]).ToArray();
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
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, bool useAllRegisteredFunctions = false, params KernelFunction[] pipeline)
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
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string requestContent, params KernelFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent, false, pipeline);
        }

        /// <summary>
        /// 创建请求实体
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="arguments"></param>
        /// <param name="useAllRegisteredFunctions">是否使用所有已经注册、创建过的 Function</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, KernelArguments arguments,
            bool useAllRegisteredFunctions = false, params KernelFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegisteredFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new KernelFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantToRun, iWantTo.UserId, iWantTo.ModelName, arguments, iWantToRun.PromptConfigParameter,
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
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, KernelArguments contextVariables, params KernelFunction[] pipeline)
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
            request.TempAiArguments ??= new SenparcAiArguments();
            request.TempAiArguments.KernelArguments.Set(key, value);
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
            request.StoreAiArguments.KernelArguments.Set(key, value);
            return request;
        }


        /// <summary>
        /// 获取上下文的值
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
        /// 获取上下文的值
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
            //var function = iWanToRun.KernelFunction;

            var prompt = request.RequestContent;
            var functionPipline = request.FunctionPipeline;
            //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

            //注意：只要使用了 Plugin 和 Function，并且包含输入标识，就需要使用上下文

            iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
            var storedArguments = iWanToRun.StoredAiArguments.KernelArguments;
            var tempArguments = request.TempAiArguments?.KernelArguments;

            FunctionResult? functionResult;

            var result = new SenparcKernelAiResult(iWanToRun, inputContent: null);

            if (tempArguments != null && tempArguments.Count() != 0)
            {
                //输入特定的本次请求临时上下文
                functionResult = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), tempArguments);
                result.InputContext = new SenparcAiArguments(tempArguments);
            }
            else if (!prompt.IsNullOrEmpty())
            {
                //输入纯文字
                if (functionPipline?.Length > 0)
                {
                    tempArguments = new KernelArguments();
                    tempArguments["INPUT"] = prompt;

                    functionResult = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), tempArguments);
                }
                else
                {
                    //注意：此处即使直接输入 prompt 作为第一个 String 参数，也会被封装到 Context，
                    //      并赋值给 Key 为 INPUT 的参数
                    //var kernelFunction = iWanToRun.CreateFunctionFromPrompt(prompt ?? "").function;
                    functionResult = await kernel.InvokePromptAsync(prompt ?? "", storedArguments);
                }

                result.InputContent = prompt;
            }
            else
            {
                //输入缓存中的上下文
                //botAnswer = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), storedArguments);
                functionResult = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), storedArguments);
                result.InputContext = new SenparcAiArguments(storedArguments);
            }

            result.InputContent = prompt;
            result.Output = functionResult.GetValue<string>()?.TrimStart('\n') ?? "";
            result.Result = functionResult;
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
        public static async Task<SenaprcAiResult<T>> RunAsync<T>(this IWantToRun iWanToRun, params KernelFunction[] pipeline)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            var result = new SenaprcAiResult<T>(iWanToRun, inputContent: null);

            var kernelResult = await kernel.InvokeAsync(pipeline.FirstOrDefault());

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