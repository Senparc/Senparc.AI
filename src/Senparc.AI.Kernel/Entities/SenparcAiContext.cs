using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Senparc.AI.Kernel.Entities
{
    public class SenparcAiContext : IAiContext<ContextVariables>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ContextVariables ExtendContext { get; set; }

        /// <summary>
        /// <inheritdoc/>>
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Context
        {
            get => ExtendContext;
            set
            {
                if (value is not ContextVariables)
                {
                    throw new SenparcAiException("Context 类型必须为 ContextVariables");
                }
                ExtendContext = (ContextVariables)value;
            }
        }

        public SenparcAiContext() : this(null)
        {
        }

        public SenparcAiContext(ContextVariables subContext)
        {
            ExtendContext = subContext;
        }
    }
}
