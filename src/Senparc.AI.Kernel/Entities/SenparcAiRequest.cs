/**
Last Modified: 20231207 - fixed Chinese encoding issues
Modified By FelixJ
*/

using Microsoft.SemanticKernel;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public record SenparcAiRequest : IAiRequest<SenparcAiArguments>
    {
        /// <summary>
        /// IWanToRun
        /// </summary>
        public IWantToRun IWantToRun { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string UserId { get; set; }
        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        //public string ModelName { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string? RequestContent { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PromptConfigParameter ParameterConfig { get; set; }

        /// <summary>
        /// Temporary context for a single request
        /// </summary>
        public SenparcAiArguments TempAiArguments { get; set; }

        /// <summary>
        /// in IWantTo context cached inside
        /// </summary>
        public SenparcAiArguments StoreAiArguments => IWantToRun.StoredAiArguments;
        /// <summary>
        /// Function
        /// </summary>
        public KernelFunction[] FunctionPipeline { get; set; }
        ///// <summary>
        ///// Rqesut.ContextVariables parameters are not saved to the context cache
        ///// </summary>
        //public ContextVariables TempContextVariables => TempAiContext?.Context as ContextVariables;
        ///// <summary>
        ///// whether to store context(ContextVariables object)
        ///// </summary>
        //public bool StoreContext => AiContext.StoreToContainer;

        public SenparcAiRequest(IWantToRun iWantToRun, string userId, string requestContent,PromptConfigParameter parameterConfig, params KernelFunction[] pipeline)
        {
            IWantToRun = iWantToRun;
            UserId = userId;
            RequestContent = requestContent;
            ParameterConfig = parameterConfig;
            TempAiArguments = new SenparcAiArguments();
            FunctionPipeline = pipeline;
        }

        public SenparcAiRequest(IWantToRun iWantToRun, string userId, KernelArguments contextVariables, PromptConfigParameter parameterConfig, params KernelFunction[] pipeline)
        {
            IWantToRun = iWantToRun;
            UserId = userId;
            ParameterConfig = parameterConfig;
            TempAiArguments = new SenparcAiArguments(contextVariables);
            FunctionPipeline = pipeline;
        }

    }
}