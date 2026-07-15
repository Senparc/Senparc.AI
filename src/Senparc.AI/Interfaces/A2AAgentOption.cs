using System;
using System.Collections.Generic;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// A2A 工具绑定模式。
    /// </summary>
    public enum A2AToolBindingMode
    {
        /// <summary>
        /// 本地函数代理：模型调用本地函数，本进程再转发到 A2A Agent。
        /// </summary>
        LocalFunctionProxy = 0,

        /// <summary>
        /// 预留：未来可扩展为原生 A2A Agent 调用模式。
        /// </summary>
        NativeA2AAgent = 1
    }

    /// <summary>
    /// A2A Agent 配置。
    /// </summary>
    public class A2AAgentOption
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AgentName { get; set; }
        public string? BaseUrl { get; set; }
        public string? LocalBaseUrl { get; set; }
        public string? PublicBaseUrl { get; set; }
        public string AgentCardPath { get; set; } = "/.well-known/agent-card.json";
        public string? SystemPrompt { get; set; }
        public string? AuthorizationBearerToken { get; set; }
        public string? ToolBindingMode { get; set; }
        public bool RequirePublicUrl { get; set; } = false;
        public List<string> PreferredBindings { get; set; } = new List<string>();
        public List<string> AllowedSkills { get; set; } = new List<string>();

        public A2AToolBindingMode GetBindingMode()
        {
            return Enum.TryParse<A2AToolBindingMode>(ToolBindingMode, ignoreCase: true, out var parsed)
                ? parsed
                : A2AToolBindingMode.LocalFunctionProxy;
        }
    }
}
