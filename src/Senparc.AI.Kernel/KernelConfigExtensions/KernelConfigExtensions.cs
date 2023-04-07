using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Helpers;
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

        public static SenparcAiRequest GetRequest(this IWantToRun iWantToRun, string requestContent, bool useAllRegistedFunctions = false, params ISKFunction[] pipeline)
        {
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            if (useAllRegistedFunctions && iWantToRun.Functions.Count > 0)
            {
                //合并已经注册的对象
                pipeline = iWantToRun.Functions.Union(pipeline ?? new ISKFunction[0]).ToArray();
            }
            var request = new SenparcAiRequest(iWantTo.UserId, iWantTo.ModelName, requestContent, iWantToRun.PromptConfigParameter, pipeline);
            return request;
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

            iWanToRun.AiContext ??= new SenparcAiContext();
            var prompt = request.RequestContent;
            var functionPipline = request.FunctionPipeline;

            //TODO；单独控制 Context
            var context = iWanToRun.AiContext.SubContext;
            //设置最新的人类对话
            context.Set("human_input", prompt);

            var botAnswer = await kernel.RunAsync(context, functionPipline);

            //获取历史信息
            var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);
            if (!context.Get(serviceId, out string history))
            {
                history = "";
            }
            //添加新信息
            history += $"\nHuman: {prompt}\nMelody: {botAnswer}\n";
            //设置历史信息
            context.Set("history", history);

            var result = new SenaprcContentAiResult()
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
