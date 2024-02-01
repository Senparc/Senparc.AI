/**
Last Modified: 20231207 - 修复中文乱码
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
        /// 单次请求的临时上下文
        /// </summary>
        public SenparcAiArguments TempAiArguments { get; set; }

        /// <summary>
        /// 在 IWantTo 里面缓存的上下文
        /// </summary>
        public SenparcAiArguments StoreAiArguments => IWantToRun.StoredAiArguments;
        /// <summary>
        /// Function
        /// </summary>
        public KernelFunction[] FunctionPipeline { get; set; }
        ///// <summary>
        ///// Rqesut.ContextVariables 参数不会保存到上下文缓存中
        ///// </summary>
        //public ContextVariables TempContextVariables => TempAiContext?.Context as ContextVariables;
        ///// <summary>
        ///// 是否储存上下文（ContextVariables 对象）
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