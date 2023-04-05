using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Helpers;
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
        private readonly IKernel _kernel;

        public SemanticAiHandler(SemanticKernelHelper semanticAiHelper = null)
        {
            SemanticKernelHelper = semanticAiHelper ?? new SemanticKernelHelper();
            _kernel = SemanticKernelHelper.GetKernel();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <returns></returns>
        public SenparcAiResult Run(SenparcAiRequest request)
        {
            //TODO:此方法暂时还不能用
            SemanticKernelHelper.Config(request.UserId, request.ModelName, _kernel);

            var senparcAiResult = new SenparcAiResult();
            return senparcAiResult;
        }


        public async Task<IWantToRun> ChatConfigAsync(PromptConfigParameter promptConfigParameter, string userId, string modelName = "text-davinci-003")
        {
            var iWantToRun = await this.IWantTo()
                                    .Config(userId, modelName)
                                    .RegisterSemanticFunctionAsync(promptConfigParameter);
            return iWantToRun;
        }

        public async Task<SenparcAiResult> ChatAsync(IWantToRun iWantToRun, SenparcAiRequest request/*, string skillName, string functionName, string skPrompt = null*/)
        {
            //var parameter = new PromptConfigParameter()
            //{
            //    MaxTokens = 2000,
            //    Temperature = 0.7,
            //    TopP = 0.5,
            //};

            var aiResult = await iWantToRun.RunAsync(request);

            return aiResult;
        }
    }

}


