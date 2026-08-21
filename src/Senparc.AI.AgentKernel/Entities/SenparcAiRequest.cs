/**
Last Modified: 20231207 - fixed Chinese encoding issues
Modified By FelixJ
*/

using Microsoft.SemanticKernel;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Handlers;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Kernels;
using Microsoft.Agents.AI;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.AgentKernel
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
        public AIFunction[] FunctionPipeline { get; set; }
        ///// <summary>
        ///// Rqesut.ContextVariables parameters are not saved to the context cache
        ///// </summary>
        //public ContextVariables TempContextVariables => TempAiContext?.Context as ContextVariables;
        ///// <summary>
        ///// whether to store context(ContextVariables object)
        ///// </summary>
        //public bool StoreContext => AiContext.StoreToContainer;

        public AgentSession AgentSession { get; set; }

        /// <summary>
        /// parameterplaceholder prefix
        /// </summary>
        public string ArgumentPrefix { get; set; } = "{{";
        /// <summary>
        /// parameterplaceholder suffix
        /// </summary>
        public string ArgumentSuffix { get; set; } = "}}";

        public SenparcAiRequest(IWantToRun iWantToRun, string userId, string requestContent, PromptConfigParameter parameterConfig, AgentSession session, params AIFunction[] pipeline)
        {
            IWantToRun = iWantToRun;
            UserId = userId;
            RequestContent = requestContent;
            ParameterConfig = parameterConfig;
            TempAiArguments = new SenparcAiArguments();
            AgentSession = session;
            FunctionPipeline = pipeline;
        }

        public SenparcAiRequest(IWantToRun iWantToRun, string userId, AgentKernelArguments contextVariables, PromptConfigParameter parameterConfig, AgentSession session, params AIFunction[] pipeline)
        {
            IWantToRun = iWantToRun;
            UserId = userId;
            ParameterConfig = parameterConfig;
            TempAiArguments = new SenparcAiArguments(contextVariables);
            AgentSession = session;
            FunctionPipeline = pipeline;
        }

        /// <summary>
        /// Replace parameters from <see cref="StoreAiArguments"/> and <see cref="TempAiArguments"/> into <see cref="RequestContent"/>
        /// </summary>
        /// <param name="prefix">placeholder prefix</param>
        /// <param name="suffix">placeholder suffix</param>
        /// <returns></returns>
        public string ReplacePrompt()
        {
            string prompt = this.RequestContent;

            if (prompt.IsNullOrEmpty())
            {
                return "";
            }

            if (StoreAiArguments != null)
            {
                prompt = StoreAiArguments.AgentKernelArguments.ReplacePrompt(prompt, ArgumentPrefix, ArgumentSuffix);
            }
            if (TempAiArguments != null)
            {
                prompt = TempAiArguments.AgentKernelArguments.ReplacePrompt(prompt, ArgumentPrefix, ArgumentSuffix);
            }
            return prompt;
        }

    }
}