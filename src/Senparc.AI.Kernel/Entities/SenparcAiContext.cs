using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;

namespace Senparc.AI.Kernel.Entities
{
    public class SenparcAiContext : IAiContext<ContextVariables>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [Obsolete("请使用 ContextVariables", true)]
        public ContextVariables ExtendContext { get; set; }
        public ContextVariables ContextVariables { get; set; }

        /// <summary>
        /// <inheritdoc/>>
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Context
        {
            get => ContextVariables;
            set
            {
                if (value is not Microsoft.SemanticKernel.Orchestration.ContextVariables)
                {
                    throw new SenparcAiException("Context 类型必须为 ContextVariables");
                }
                ContextVariables = (ContextVariables)value;
            }
        }

        public SenparcAiContext() : this(null)
        {
        }

        public SenparcAiContext(ContextVariables subContext)
        {
            ContextVariables = subContext;
        }

        /// <summary>
        /// 尝试初始化 ExtendContext 上下文对象，如果已经初始化，则不进行操作
        /// </summary>
        public void TryInitExtendContext()
        {
            ContextVariables ??= new ContextVariables();
        }
    }
}
