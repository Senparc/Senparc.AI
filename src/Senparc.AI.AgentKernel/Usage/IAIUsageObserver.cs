namespace Senparc.AI.AgentKernel.Usage;

/// <summary>
/// Receives provider usage events without being part of the model response path.
/// Implementations must enqueue or otherwise hand off the event quickly.
/// </summary>
public interface IAIUsageObserver
{
    void Observe(AIUsageEvent usageEvent);
}
