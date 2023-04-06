using Microsoft.SemanticKernel.Connectors.OpenAI.TextCompletion;
using Microsoft.SemanticKernel.SemanticFunctions;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Handlers
{
    public static class KernelConfigExtension
    {
        //public static IWantTo IWantTo(this SemanticKernelHelper sKHelper)
        //{
        //    var iWantTo = new IWantTo(sKHelper);
        //    return iWantTo;
        //}

        public static IWantTo IWantTo(this SemanticAiHandler handler)
        {
            var iWantTo = new IWantTo(handler);
            return iWantTo;
        }

        public static SenparcAiRequest GetRequest(this IWantToRun iWantToRun, string requestContent)
        {
            var iWantTo = iWantToRun.IWantTo;
            var request = new SenparcAiRequest(iWantTo.UserId, iWantTo.ModelName, requestContent, iWantToRun.PromptConfigParameter);
            return request;
        }

        public static IWantToConfig ConfigModel(this IWantTo iWantTo, ConfigModel configModel, string userId, string modelName)
        {
            var kernel = configModel switch
            {
                AI.ConfigModel.TextCompletion => iWantTo.SemanticKernelHelper.ConfigTextCompletion(userId, modelName),
                AI.ConfigModel.Embedding => iWantTo.SemanticKernelHelper.ConfigTextCompletion(userId, modelName),
                _ => throw new SenparcAiException("未处理当前 ConfigModel 类型：" + configModel)
            }; ;
            iWantTo.Kernel = kernel;//进行 Config 必须提供 Kernel
            iWantTo.UserId = userId;
            iWantTo.ModelName = modelName;
            return new IWantToConfig(iWantTo);
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



        public static async Task<IWantToRun> RegisterSemanticFunctionAsync(this IWantToConfig iWantToConfig, PromptConfigParameter promptConfigPara, string? skPrompt = null)
        {
            skPrompt ??= @"
ChatBot can have a conversation with you about any topic.
It can give explicit instructions or say 'I don't know' if it does not have an answer.

{{$history}}
Human: {{$human_input}}
ChatBot:";

            var promptConfig = new PromptTemplateConfig
            {
                Completion =
                    {
                        MaxTokens = promptConfigPara.MaxTokens.Value,
                        Temperature = promptConfigPara.Temperature.Value,
                        TopP = promptConfigPara.TopP.Value,
                    }
            };

            var iWantTo = iWantToConfig.IWantTo;
            var helper = iWantTo.SemanticKernelHelper;
            var handler = iWantTo.SemanticAiHandler;
            var kernel = helper.GetKernel();
            var promptTemplate = new PromptTemplate(skPrompt, promptConfig, kernel);
            var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
            //TODO:提供自定义的skillName和functionName
            var chatFunction = kernel.RegisterSemanticFunction("ChatBot", "Chat", functionConfig);

            var aiContext = new SenparcAiContext();

            var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);
            var history = "";
            aiContext.SubContext.Set(serviceId, history);

            return new IWantToRun(new IWantTo(handler))
            {
                ISKFunction = chatFunction,
                AiContext = aiContext,
                PromptConfigParameter = promptConfigPara
            };
        }

        public static async Task<SenparcAiResult> RunAsync(this IWantToRun iWanToRun, SenparcAiRequest request)
        {
            var helper = iWanToRun.IWantTo.SemanticKernelHelper;
            var kernel = helper.Kernel;
            var function = iWanToRun.ISKFunction;
            var context = iWanToRun.AiContext.SubContext;
            var iWantTo = iWanToRun.IWantTo;
            var prompt = request.RequestContent;

            //设置最新的人类对话
            context.Set("human_input", prompt);

            var botAnswer = await kernel.RunAsync(context, function);

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

            var result = new SenparcAiResult()
            {
                Input = prompt,
                Output = botAnswer.Result,
                LastException = botAnswer.LastException
            };
            return result;
        }
    }

}
