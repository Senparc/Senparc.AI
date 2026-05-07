using Microsoft.SemanticKernel;
using Senparc.AI.Interfaces;

namespace Senparc.AI.Kernel.Entities;

public class SenparcAiArguments : IAiContext
{
    public KernelArguments KernelArguments { get; set; } = new();

    public IDictionary<string, object?> Context
    {
        get => KernelArguments;
        set
        {
            var args = new KernelArguments();
            foreach (var item in value)
            {
                args[item.Key] = item.Value;
            }
            KernelArguments = args;
        }
    }

    public SenparcAiArguments()
    {
    }

    public SenparcAiArguments(KernelArguments kernelArguments)
    {
        KernelArguments = kernelArguments;
    }
}
