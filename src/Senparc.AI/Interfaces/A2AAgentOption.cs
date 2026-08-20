using System;
using System.Collections.Generic;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// A2A tool binding mode.
    /// </summary>
    public enum A2AToolBindingMode
    {
        /// <summary>
        /// Local function proxy: the model calls a local function, and this process forwards the request to the A2A Agent.
        /// </summary>
        LocalFunctionProxy = 0,

        /// <summary>
        /// Reserved for a future native A2A Agent invocation mode.
        /// </summary>
        NativeA2AAgent = 1
    }

    /// <summary>
    /// A2A Agent configuration.
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
