using Senparc.AI.Interfaces;

namespace Senparc.AI.AgentKernel.Usage;

/// <summary>
/// Optional metadata carried with provider usage events.
/// </summary>
public sealed record AIUsageContext
{
    public string? Source { get; init; }
    public string? TenantId { get; init; }
    public string? UserId { get; init; }
    public string? SessionId { get; init; }
    public string? ModelId { get; init; }
    public string? ModelName { get; init; }
    public string? DeploymentName { get; init; }
    public AiPlatform? Platform { get; init; }
}
