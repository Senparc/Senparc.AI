using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public record class SenparcAiRequest : IAiRequest<SenparcAiContext>
    {
        /// <summary>
        /// IWanToRun
        /// </summary>
        public IWantToRun IWantToRun { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ModelName { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string RequestContent { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PromptConfigParameter ParameterConfig { get; set; }

        /// <summary>
        /// 单次请求的临时上下文
        /// </summary>
        public SenparcAiContext TempAiContext { get; set; }

        /// <summary>
        /// 在 IWantTo 里面缓存的上下文
        /// </summary>
        public SenparcAiContext StoreAiContext => IWantToRun.StoredAiContext;
        /// <summary>
        /// Function
        /// </summary>
        public ISKFunction[] FunctionPipeline { get; set; }
        ///// <summary>
        ///// Rqesut.ContextVariables 参数不会保存到上下文缓存中
        ///// </summary>
        public ContextVariables TempContextVariables { get; set; }
        ///// <summary>
        ///// 是否储存上下文（ContextVariables 对象）
        ///// </summary>
        //public bool StoreContext => AiContext.StoreToContainer;

        public SenparcAiRequest(IWantToRun iWantToRun, string userId, string modelName, string requestContent,PromptConfigParameter parameterConfig, params ISKFunction[] pipeline)
        {
            IWantToRun = iWantToRun;
            UserId = userId;
            ModelName = modelName;
            RequestContent = requestContent;
            ParameterConfig = parameterConfig;
            TempAiContext = new SenparcAiContext();
            FunctionPipeline = pipeline;
        }

        public SenparcAiRequest(IWantToRun iWantToRun, string userId, string modelName, ContextVariables contextVariables, PromptConfigParameter parameterConfig, params ISKFunction[] pipeline)
        {
            IWantToRun = iWantToRun;
            UserId = userId;
            ModelName = modelName;
            ParameterConfig = parameterConfig;
            TempAiContext = new SenparcAiContext();
            FunctionPipeline = pipeline;
        }

    }
}