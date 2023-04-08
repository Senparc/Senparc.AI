using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class SenparcAiRequest : IAiRequest<ContextVariables>
    {

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
        /// 上下文
        /// </summary>
        public IAiContext<ContextVariables> IAiContext { get; set; }
        /// <summary>
        /// Function
        /// </summary>
        public ISKFunction[] FunctionPipeline { get; set; }
        /// <summary>
        /// ContextVariables，如果 StoreContext 为 true，则会覆盖当前 iWanToRun 中储存的 Context
        /// </summary>
        public ContextVariables ContextVariables { get; set; }
        /// <summary>
        /// 是否储存上下文（ContextVariables 对象）
        /// </summary>
        public bool StoreContext { get; set; }

        public SenparcAiRequest(string userId, string modelName, string requestContent,PromptConfigParameter parameterConfig, bool storeContext=false, params ISKFunction[] pipeline)
        {
            UserId = userId;
            ModelName = modelName;
            RequestContent = requestContent;
            ParameterConfig = parameterConfig;
            IAiContext = new SenparcAiContext();
            StoreContext = storeContext;
            FunctionPipeline = pipeline;
        }

        public SenparcAiRequest(string userId, string modelName, ContextVariables contextVariables, PromptConfigParameter parameterConfig, bool storeContext = false, params ISKFunction[] pipeline)
        {
            UserId = userId;
            ModelName = modelName;
            ContextVariables = contextVariables;
            ParameterConfig = parameterConfig;
            IAiContext = new SenparcAiContext();
            StoreContext = storeContext;
            FunctionPipeline = pipeline;
        }

    }
}