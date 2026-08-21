using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.Mcp
{
    /// <summary>
    /// MCP toolset preparation result.
    /// </summary>
    public sealed class McpToolsetResult : IAsyncDisposable
    {
        public required McpServerOption ServerOption { get; init; }

        public required McpToolBindingMode BindingMode { get; init; }

        public required string ResolvedSseUrl { get; init; }

        public required Uri McpUri { get; init; }

        public required IReadOnlyList<McpClientTool> DiscoveredMcpTools { get; init; }

        public required IReadOnlyList<string> DiscoveredToolNames { get; init; }

        public required IReadOnlyList<AITool> ChatTools { get; init; }

        public string? ToolDiscoveryError { get; init; }

        /// <summary>
        /// MCP client used to execute tool calls in LocalFunctionProxy mode.
        /// </summary>
        internal McpClient? RuntimeMcpClient { get; init; }

        public async ValueTask DisposeAsync()
        {
            if (RuntimeMcpClient is not null)
            {
                await RuntimeMcpClient.DisposeAsync();
            }
        }
    }
}
