using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
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

        public static IWantToConfig ConfigModel(this IWantToConfig iWantToConfig, ConfigModel configModel, string userId, string modelName)
        {
            var iWantTo = iWantToConfig.IWantTo;
            var existedKernelBuilder = iWantToConfig.IWantTo.KernelBuilder;
            var kernelBuilder = configModel switch
            {
                AI.ConfigModel.TextCompletion => iWantTo.SemanticKernelHelper.ConfigTextCompletion(userId, modelName, existedKernelBuilder),
                AI.ConfigModel.TextEmbedding => iWantTo.SemanticKernelHelper.ConfigTextEmbeddingGeneration(userId, modelName, existedKernelBuilder),
                _ => throw new SenparcAiException("未处理当前 ConfigModel 类型：" + configModel)
            };
            iWantTo.KernelBuilder = kernelBuilder;//进行 Config 必须提供 Kernel
            iWantTo.UserId = userId;
            iWantTo.ModelName = modelName;
            return iWantToConfig;
        }


        /// <summary>
        /// 添加 TextCompletion 配置
        /// </summary>
        /// <param name="iWantToConfig"></param>
        /// <param name="modelName">如果为 null，则从先前配置中读取</param>
        /// <returns></returns>
        /// <exception cref="SenparcAiException"></exception>
        public static IWantToConfig AddTextCompletion(this IWantToConfig iWantToConfig, string? modelName = null)
        {
            var aiPlatForm = iWantToConfig.IWantTo.SemanticKernelHelper.AiSetting.AiPlatform;
            var kernel = iWantToConfig.IWantTo.Kernel;
            var skHelper = iWantToConfig.IWantTo.SemanticKernelHelper;
            var aiSetting = skHelper.AiSetting;
            var userId = iWantToConfig.IWantTo.UserId;
            modelName ??= iWantToConfig.IWantTo.ModelName;
            var serviceId = skHelper.GetServiceId(userId, modelName);

            //TODO 需要判断 Kernel.TextCompletionServices.ContainsKey(serviceId)，如果存在则不能再添加

            kernel.Config.AddTextCompletionService(serviceId, k =>
                aiPlatForm switch
                {
                    AiPlatform.OpenAI => new OpenAITextCompletion(modelName, aiSetting.ApiKey, aiSetting.OrgaizationId),

                    AiPlatform.AzureOpenAI => new AzureTextCompletion(modelName, aiSetting.AzureEndpoint, aiSetting.ApiKey, aiSetting.AzureOpenAIApiVersion),

                    _ => throw new SenparcAiException($"没有处理当前 {nameof(AiPlatform)} 类型：{aiPlatForm}")
                }
            );

            return iWantToConfig;
        }

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
        /// <param name="useAllRegistedFunctions">是否使用所有已经注册、创建过的 Function</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string requestContent, bool useAllRegistedFunctions = false, bool storeContext = false, params ISKFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegistedFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new ISKFunction[0]).ToArray();
            }

            var request = new SenparcAiRequest(iWantTo.UserId, iWantTo.ModelName, requestContent, iWantToRun.PromptConfigParameter, storeContext, pipeline);
            return request;
        }

        /// <summary>
        /// 创建请求实体（不使用所有已经注册、创建过的 Function，也不储存行下文）
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="requestContent"></param>
        /// <param name="useAllRegistedFunctions"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string requestContent, params ISKFunction[] pipeline)
        {
            return CreateRequest(iWantToRun, requestContent, false, false, pipeline);
        }

        /// <summary>
        /// 创建请求实体
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="contextVariables"></param>
        /// <param name="useAllRegistedFunctions">是否使用所有已经注册、创建过的 Function</param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, ContextVariables contextVariables, bool useAllRegistedFunctions = false, bool storeContext = false, params ISKFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegistedFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new ISKFunction[0]).ToArray();
            }
            var request = new SenparcAiRequest(iWantTo.UserId, iWantTo.ModelName, contextVariables, iWantToRun.PromptConfigParameter, storeContext, pipeline);
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
            return CreateRequest(iWantToRun, contextVariables, false, false, pipeline);
        }

        #endregion

        #region 运行阶段，或对生成后的 Kernel 进行补充设置

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<SenaprcContentAiResult> RunAsync(this IWantToRun iWanToRun, SenparcAiRequest request)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.ISKFunction;

            var prompt = request.RequestContent;
            var functionPipline = request.FunctionPipeline;

            //上下文
            //获取历史信息
            var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);
            var storeContext = request.StoreContext;

            if (storeContext && request.ContextVariables != null)
            {
                iWanToRun.AiContext.SubContext = request.ContextVariables;
            }

            iWanToRun.AiContext ??= new SenparcAiContext();
            var context = iWanToRun.AiContext.SubContext;

            var storeContextExisted = false;//当 StoreContext 时，是否发现已有上下文存在
            if (storeContext)
            {
                string history = null;
                if (context.Get(serviceId, out history))
                {
                    storeContextExisted = true;
                }
                context.Set("human_input", prompt);
            }

            SKContext? botAnswer;
            if (!storeContextExisted &&
                !request.RequestContent.IsNullOrEmpty() &&
                (request.ContextVariables == null || request.ContextVariables.Count() == 0))
            {
                //输入文字
                botAnswer = await kernel.RunAsync(request.RequestContent, functionPipline);
            }
            else if (request.ContextVariables != null)
            {
                //输入指定的上下文
                botAnswer = await kernel.RunAsync(request.ContextVariables, functionPipline);
            }
            else
            {
                //输入指定的上下文

                //设置最新的人类对话
                botAnswer = await kernel.RunAsync(context, functionPipline);
            }

            if (storeContext)
            {
                string history = null;
                if (!context.Get(serviceId, out history))
                {
                    history = "";
                }

                //添加新信息
                history += $"\nHuman: {prompt}\nBot: {botAnswer}\n";
                //设置历史信息
                context.Set("history", history);
            }

            var result = new SenaprcContentAiResult(iWanToRun)
            {
                Input = prompt,
                Output = botAnswer.Result,
                Result = botAnswer,
                LastException = botAnswer.LastException
            };
            return result;
        }

        #endregion

    }

}
