/**
Last Modified: 20231207 - 修复中文乱码
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
        public AIFunction[] FunctionPipeline { get; set; }
        ///// <summary>
        ///// Rqesut.ContextVariables 参数不会保存到上下文缓存中
        ///// </summary>
        //public ContextVariables TempContextVariables => TempAiContext?.Context as ContextVariables;
        ///// <summary>
        ///// 是否储存上下文（ContextVariables 对象）
        ///// </summary>
        //public bool StoreContext => AiContext.StoreToContainer;

        public AgentSession AgentSession { get; set; }

        /// <summary>
        /// 参数占位符前缀
        /// </summary>
        public string ArgumentPrefix { get; set; } = "{{";
        /// <summary>
        /// 参数占位符后缀
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
        /// 替换 <see cref="StoreAiArguments"> 及 <see cref="TempAiArguments"/> 中的参数到 <see cref="RequestContent"/>
        /// </summary>
        /// <param name="prefix">占位符前缀</param>
        /// <param name="suffix">占位符后缀</param>
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