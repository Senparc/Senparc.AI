namespace Senparc.AI.AgentKernel.Usage;

/// <summary>
/// Holds the optional process-wide observer used by manually constructed AgentKernel handlers.
/// DI-based callers should prefer <see cref="IAIUsageObserver"/> on the builder.
/// </summary>
public static class AgentKernelUsageRegistry
{
    public static IAIUsageObserver? DefaultObserver { get; set; }
}
