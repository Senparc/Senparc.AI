using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// SenmanticKernel 处理器
    /// </summary>
    public class SemanticAiHandler :
        IAiHandler<SenparcAiRequest, SenparcAiResult, SenparcAiContext>
    {
        public SemanticKernelHelper SemanticKernelHelper { get; set; }
        private IKernel _kernel => SemanticKernelHelper.GetKernel();



        public SemanticAiHandler(SemanticKernelHelper? semanticAiHelper = null)
        {
            SemanticKernelHelper = semanticAiHelper ?? new SemanticKernelHelper();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <returns></returns>
        public SenparcAiResult Run(SenparcAiRequest request, ISenparcAiSetting senparcAiSetting = null)
        {
            //TODO:未正式使用

            //TODO:此方法暂时还不能用
            SemanticKernelHelper.ConfigTextCompletion(request.UserId, request.ModelName, senparcAiSetting, null);

            var senparcAiResult = new SenparcAiResult(new IWantToRun(new IWantToBuild(new IWantToConfig(new IWantTo()))), request.RequestContent);
            return senparcAiResult;
        }

        public (IWantToRun iWantToRun, ISKFunction chatFunction) ChatConfig(PromptConfigParameter promptConfigParameter, string userId, string modelName = "text-davinci-003")
        {
            var result = this.IWantTo()
                                    .ConfigModel(ConfigModel.TextCompletion, userId, modelName)
                                    .BuildKernel()
                                    .RegisterSemanticFunction("ChatBot", "Chat", promptConfigParameter);

            return result;
        }

        public async Task<SenparcAiResult> ChatAsync(IWantToRun iWantToRun, string prompt)
        {
            //var function = iWantToRun.Kernel.Skills.GetSemanticFunction("Chat");
            //request.FunctionPipeline = new[] { function };

            var request = iWantToRun.CreateRequest(true);

            //历史记录
            //初始化对话历史（可选）
            if (!request.GetStoredContext("history", out var history))
            {
                request.SetStoredContext("history", "");
            }

            //本次记录
            request.SetStoredContext("human_input", prompt);

            var newRequest = request with { RequestContent = null };

            //运行
            var aiResult = await iWantToRun.RunAsync(newRequest);

            //记录对话历史（可选）
            request.SetStoredContext("history", history + $"\nHuman: {prompt}\nBot: {request.RequestContent}");

            return aiResult;
        }


    }

}


