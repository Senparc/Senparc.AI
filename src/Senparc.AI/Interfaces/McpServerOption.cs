using System;
using System.Collections.Generic;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// MCP tool binding mode.
    /// </summary>
    public enum McpToolBindingMode
    {
        /// <summary>
        /// Local function proxy: the model calls a local function, and this process forwards it to MCP.
        /// </summary>
        LocalFunctionProxy = 0,

        /// <summary>
        /// Hosted MCP: the model service connects directly to and calls the MCP Server.
        /// </summary>
        HostedServerTool = 1
    }

    /// <summary>
    /// MCP Server configuration.
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
