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
    /// SenmanticKernel ´¦ÀíÆ÷
    /// </summary>
    public class SemanticAiHandler :
        IAiHandler<SenparcAiRequest, SenparcAiResult, SenparcAiContext, ContextVariables>
    {
        private readonly SemanticKernelHelper _skHandler;
        private readonly IKernel _kernel;

        public SemanticAiHandler(SemanticKernelHelper semanticAiHandler=null)
        {
            _skHandler = semanticAiHandler?? new SemanticKernelHelper();
            _kernel = _skHandler.GetKernel();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <returns></returns>
        public SenparcAiResult Run(SenparcAiRequest request)
        {
            _skHandler.Config(request.UserId, request.ModelName, _kernel);

            var senparcAiResult = new SenparcAiResult();
            return senparcAiResult;
        }

        public async Task<SenparcAiResult> ChatAsync(SenparcAiRequest request, string skillName, string functionName, string skPrompt = null)
        {
            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

           var aiResult = _skHandler.IWantTo()
                       .Config("Jeffrey", "text-davinci-003")
                       .RegisterSemanticFunctionAsync(parameter).GetAwaiter().GetResult()
                       .RunAsync("What's the population on the earth?").GetAwaiter().GetResult();

            return aiResult;

            //var promptTemplate = new PromptTemplate(skPrompt, promptConfig, _kernel);
            //var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);

            //var chatFunction = _kernel.RegisterSemanticFunction(skillName, functionName, functionConfig);

            //var subRequest = request as SenparcAiRequest;
            //var context = subRequest.IAiContext;

            //var botAnswer = await _kernel.RunAsync(context.SubContext, chatFunction);

            //var result = new SenparcAiResult()
            //{
            //    Output = botAnswer.Result
            //};
            //return result;
        }
    }

}


