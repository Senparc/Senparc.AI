using System;
using System.Collections.Generic;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// MCP 工具绑定模式。
    /// </summary>
    public enum McpToolBindingMode
    {
        /// <summary>
        /// 本地函数代理：模型调用本地函数，本进程再转发到 MCP。
        /// </summary>
        LocalFunctionProxy = 0,

        /// <summary>
        /// Hosted MCP：模型服务端直接连接并调用 MCP Server。
        /// </summary>
        HostedServerTool = 1
    }

    /// <summary>
    /// MCP Server 配置。
    /// </summary>
    public class McpServerOption
    {
        public string Name { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public string? SseUrl { get; set; }
        public string? LocalSseUrl { get; set; }
        public string? PublicBaseUrl { get; set; }
        public string? ServerDescription { get; set; }
        public string? SystemPrompt { get; set; }
        public string? AuthorizationBearerToken { get; set; }
        public string? ToolBindingMode { get; set; }
        public bool RequirePublicUrl { get; set; } = true;
        public List<string> AllowedTools { get; set; } = new List<string>();

        public McpToolBindingMode GetBindingMode()
        {
            return Enum.TryParse<McpToolBindingMode>(ToolBindingMode, ignoreCase: true, out var parsed)
                ? parsed
                : McpToolBindingMode.LocalFunctionProxy;
        }
    }
}
