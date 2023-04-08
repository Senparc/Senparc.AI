using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
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
        IAiHandler<SenparcAiRequest, SenparcAiResult, SenparcAiContext, ContextVariables>
    {
        public SemanticKernelHelper SemanticKernelHelper { get; set; }
        private IKernel _kernel => SemanticKernelHelper.GetKernel();



        public SemanticAiHandler(SemanticKernelHelper semanticAiHelper = null)
        {
            SemanticKernelHelper = semanticAiHelper ?? new SemanticKernelHelper();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <returns></returns>
        public SenparcAiResult Run(SenparcAiRequest request)
        {
            //TODO:未正式使用

            //TODO:此方法暂时还不能用
            SemanticKernelHelper.ConfigTextCompletion(request.UserId, request.ModelName, null);

            var senparcAiResult = new SenparcAiResult(new IWantToRun(new IWantToBuild(new IWantToConfig(new IWantTo()))));
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

        public async Task<SenparcAiResult> ChatAsync(IWantToRun iWantToRun, SenparcAiRequest request)
        {
            //var function = iWantToRun.Kernel.Skills.GetSemanticFunction("Chat");
            //request.FunctionPipeline = new[] { function };
            var aiResult = await iWantToRun.RunAsync(request);
            return aiResult;
        }


    }

}


