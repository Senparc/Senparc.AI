using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;

namespace Senparc.AI.AgentKernel.Mcp
{
    /// <summary>
    /// MCP 工具集准备结果。
    /// </summary>
    public sealed class McpToolsetResult
    {
        public required McpServerOption ServerOption { get; init; }

        public required McpToolBindingMode BindingMode { get; init; }

        public required string ResolvedSseUrl { get; init; }

        public required Uri McpUri { get; init; }

        public required IReadOnlyList<McpClientTool> DiscoveredMcpTools { get; init; }

        public required IReadOnlyList<string> DiscoveredToolNames { get; init; }

        public required IReadOnlyList<AITool> ChatTools { get; init; }

        public string? ToolDiscoveryError { get; init; }
    }
}
