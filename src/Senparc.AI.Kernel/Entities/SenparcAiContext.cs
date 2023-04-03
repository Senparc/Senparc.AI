using Microsoft.SemanticKernel.Orchestration;
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
        public object Context { get; set; }
    }
}
