using Microsoft.SemanticKernel.SemanticFunctions;
using Senparc.AI.Entities;
using Senparc.AI.Kernel.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Senparc.AI.Kernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        #region Memory 相关

        public static async Task<IWantToRun> RegisterSemanticFunctionAsync(this IWantToRun iWantToRun, PromptConfigParameter promptConfigPara, string? skPrompt = null)
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


            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;
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

            iWantToRun.ISKFunction = chatFunction;
            iWantToRun.AiContext = aiContext;
            iWantToRun.PromptConfigParameter = promptConfigPara;

            return iWantToRun;

        }

        public static async Task<SenparcAiResult> RunAsync(this IWantToRun iWanToRun, SenparcAiRequest request)
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWantTo.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            var function = iWanToRun.ISKFunction;
            var context = iWanToRun.AiContext.SubContext;
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

        public static IWantToRun MemorySaveInformation(this IWantToRun iWantToRun,
            string collection,
            string text,
            string id,
            string? description = null,
            CancellationToken cancel = default)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            //var kernel = helper.GetKernel();
            var memory = helper.GetMemory();
            var task = helper.MemorySaveInformationAsync(memory, collection, text, id, description, cancel);
            helper.AddMemory(task);

            return iWantToRun;
        }

        public static IWantToRun MemorySaveReference(this IWantToRun iWantToRun,
               string collection,
               string text,
               string externalId,
               string externalSourceName,
               string? description = null,
               CancellationToken cancel = default)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            //var kernel = helper.GetKernel();
            var memory = helper.GetMemory();
            var task = helper.MemorySaveReferenceAsync(memory, collection, text, externalId, externalSourceName, description, cancel);
            helper.AddMemory(task);

            return iWantToRun;
        }

        public static IWantToRun MemoryStoreExexute(this IWantToRun iWantToRun)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            helper.ExecuteMemory();
            return iWantToRun;
        }

        /// <summary>
        /// Memory 查询
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="collection">Collection to search</param>
        /// <param name="query">What to search for</param>
        /// <param name="limit">How many results to return</param>
        /// <param name="minRelevanceScore">Minimum relevance score, from 0 to 1, where 1 means exact match.</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>Memories found</returns>
        /// <returns></returns>
        public static async Task<SenaprcAiResult_MemoryQuery> MemorySearchAsync(this IWantToRun iWantToRun,
            string memoryCollectionName,
            string query,
            int limit = 1,
            double minRelevanceScore = 0.7,
            CancellationToken cancel = default)
        {
            var handler = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler;
            var helper = handler.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            var memory = helper.GetMemory();
            var queryResult = memory.SearchAsync(memoryCollectionName, query, limit, minRelevanceScore, cancel);

            var aiResult = new SenaprcAiResult_MemoryQuery()
            {
                Input = query,
                MemoryQueryResult = queryResult,
            };
            return aiResult;
        }

        #endregion
    }
}
