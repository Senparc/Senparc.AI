using Microsoft.SemanticKernel;
using Senparc.AI.AgentKernel.Kernels;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;

namespace Senparc.AI.AgentKernel.Entities
{
    public class SenparcAiArguments : IAiContext<AgentKernelArguments>
    {
   
        private AgentKernelArguments? _kernelArguments { get; set; }


        public AgentKernelArguments AgentKernelArguments
        {
            get => _kernelArguments ??= new AgentKernelArguments();
            set => _kernelArguments = value;
        }

        /// <summary>
        /// <inheritdoc/>>
        /// </summary>
        public ISenparcKernelArguments /*IDictionary<string, object?>*/ Context
        {
            get => AgentKernelArguments;
            set
            {
                if (value is not Senparc.AI.AgentKernel.Entities.AgentKernelArguments)
                {
                    throw new SenparcAiException("Context 类型必须为 ISenparcKernelArguments");
                }
                AgentKernelArguments = (AgentKernelArguments)value;
            }
        }

        IDictionary<string, object?> IAiContext.Context { get => Context; set => throw new NotImplementedException(); }

        public SenparcAiArguments() : this(null)
        {
        }

        public SenparcAiArguments(AgentKernelArguments subContext)
        {
            AgentKernelArguments = subContext;
        }

    }
}
