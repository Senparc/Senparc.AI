using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Senparc.AI.Helpers;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// SenmanticKernel ������
    /// </summary>
    public class SemanticAiHandler : IAiHandler<SenparcAiResult>
    {
        private readonly SemanticKernelHelper _skHandler;
        private readonly IKernel _kernel;

        public SemanticAiHandler(SemanticKernelHelper semanticAiHandler)
        {
            _skHandler = semanticAiHandler;
            _kernel = _skHandler.GetKernel();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <returns></returns>
        public SenparcAiResult Run(IAiRequest request)
        {
            _skHandler.Config(request.UserId, request.ModelName, _kernel);

            var senparcAiResult = new SenparcAiResult();
            return senparcAiResult;
        }

        public async Task<SenparcAiResult> Chatasync(IAiRequest request, string skillName, string functionName, string skPrompt = null)
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
                            MaxTokens = request.ParameterConfig.MaxTokens,
                            Temperature = request.ParameterConfig.Temperature,
                            TopP = request.ParameterConfig.TopP,
                        }
            };

            var promptTemplate = new PromptTemplate(skPrompt, promptConfig, _kernel);
            var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);

            var chatFunction = _kernel.RegisterSemanticFunction(skillName, functionName, functionConfig);

            var subRequest = request as SenparcAiRequest;
            var context = subRequest.IAiContext;

            var botAnswer = await _kernel.RunAsync(context.SubContext, chatFunction);

            var result = new SenparcAiResult()
            {
                Output = botAnswer.Result
            };

        }
    }

}


