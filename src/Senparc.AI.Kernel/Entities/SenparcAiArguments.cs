using Microsoft.SemanticKernel;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;

namespace Senparc.AI.Kernel.Entities
{
    public class SenparcAiArguments : IAiContext<KernelArguments>
    {
   
        private KernelArguments? _kernelArguments { get; set; }


        public KernelArguments KernelArguments
        {
            get => _kernelArguments ??= new KernelArguments();
            set => _kernelArguments = value;
        }

        /// <summary>
        /// <inheritdoc/>>
        /// </summary>
        public IDictionary<string, object?> Context
        {
            get => KernelArguments;
            set
            {
                if (value is not Microsoft.SemanticKernel.KernelArguments)
                {
                    throw new SenparcAiException("Context 类型必须为 IDictionary<string, object?>");
                }
                KernelArguments = (KernelArguments)value;
            }
        }

        public SenparcAiArguments() : this(null)
        {
        }

        public SenparcAiArguments(KernelArguments subContext)
        {
            KernelArguments = subContext;
        }

    }
}
