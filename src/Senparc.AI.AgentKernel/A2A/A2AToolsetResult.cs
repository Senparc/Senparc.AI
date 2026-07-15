using A2A;
using Microsoft.Extensions.AI;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.A2A
{
    /// <summary>
    /// A2A 工具集准备结果。
    /// </summary>
    public sealed class A2AToolsetResult : IAsyncDisposable
    {
        public required A2AAgentOption AgentOption { get; init; }

        public required A2AToolBindingMode BindingMode { get; init; }

        public required string ResolvedBaseUrl { get; init; }

        public required Uri BaseUri { get; init; }

        public required AgentCard AgentCard { get; init; }

        public required IReadOnlyList<string> DiscoveredSkillNames { get; init; }

        public required IReadOnlyList<AITool> ChatTools { get; init; }

        public string? CardDiscoveryError { get; init; }

        /// <summary>
        /// LocalFunctionProxy 模式下用于执行工具调用的 A2A 客户端。
        /// </summary>
        internal IA2AClient? RuntimeA2AClient { get; init; }

        internal HttpClient? RuntimeHttpClient { get; init; }

        public ValueTask DisposeAsync()
        {
            if (RuntimeA2AClient is IDisposable disposableClient)
            {
                disposableClient.Dispose();
            }

            RuntimeHttpClient?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
