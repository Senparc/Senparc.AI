using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.Mcp
{
    /// <summary>
    /// MCP 工具集准备结果。
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
        /// LocalFunctionProxy 模式下用于执行工具调用的 MCP 客户端。
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
