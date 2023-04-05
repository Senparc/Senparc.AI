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
        public ContextVariables SubContext { get; set; }
        public object Context
        {
            get => SubContext;
            set
            {
                if (value is not ContextVariables)
                {
                    throw new SenparcAiException("Context 类型必须为 ContextVariables");
                }
                SubContext = (ContextVariables)value;
            }
        }

        public SenparcAiContext()
        {
            SubContext = new ContextVariables();
        }
    }
}
