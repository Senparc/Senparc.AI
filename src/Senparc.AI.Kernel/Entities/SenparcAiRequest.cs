using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class SenparcAiRequest: IAiRequest<ContextVariables>
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
        /// иообнд
        /// </summary>
       public IAiContext<ContextVariables> IAiContext { get; set; }

        public SenparcAiRequest(string userId, string modelName, string requestContent, PromptConfigParameter parameterConfig)
        {
            UserId = userId;
            ModelName = modelName;
            RequestContent = requestContent;
            ParameterConfig = parameterConfig;
            IAiContext = new SenparcAiContext();
        }

    }
}