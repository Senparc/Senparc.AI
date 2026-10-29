using Microsoft.Extensions.AI;
using System;

namespace Senparc.AI.AgentKernel.Usage;

/// <summary>
/// Describes one completed or failed provider invocation.
/// </summary>
public sealed record AIUsageEvent
{
    public required string Operation { get; init; }
    public AIUsageContext? Context { get; init; }
    public string? Source { get; init; }
    public string? Model { get; init; }
    public UsageDetails? Usage { get; init; }
    public bool Success { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
    public Exception? Exception { get; init; }
}
