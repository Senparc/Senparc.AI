using Microsoft.Extensions.Configuration;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Senparc.AI.AgentKernel.A2A
{
    /// <summary>
    /// A2A 配置扩展。
    /// </summary>
    public static class A2AConfigurationExtensions
    {
        /// <summary>
        /// 从配置中读取 A2A Agent 列表。
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="sectionPath">默认：SenparcAiSetting:A2AAgents</param>
        /// <returns></returns>
        public static IReadOnlyList<A2AAgentOption> GetA2AAgentOptions(this IConfiguration configuration, string sectionPath = "SenparcAiSetting:A2AAgents")
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var list = new List<A2AAgentOption>();
            var agentSections = configuration.GetSection(sectionPath).GetChildren();

            foreach (var section in agentSections)
            {
                var name = section["Name"] ?? section["AgentName"] ?? "A2AAgent";
                var baseUrl = section["BaseUrl"];
                var localBaseUrl = section["LocalBaseUrl"];

                var preferredBindings = section.GetSection("PreferredBindings")
                    .GetChildren()
                    .Select(z => z.Value?.Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .Cast<string>()
                    .ToList();

                var allowedSkills = section.GetSection("AllowedSkills")
                    .GetChildren()
                    .Select(z => z.Value?.Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .Cast<string>()
                    .ToList();

                if (string.IsNullOrWhiteSpace(baseUrl) && string.IsNullOrWhiteSpace(localBaseUrl))
                {
                    continue;
                }

                list.Add(new A2AAgentOption
                {
                    Name = name,
                    AgentName = section["AgentName"],
                    Description = section["Description"],
                    BaseUrl = baseUrl,
                    LocalBaseUrl = localBaseUrl,
                    PublicBaseUrl = section["PublicBaseUrl"],
                    AgentCardPath = section["AgentCardPath"] ?? "/.well-known/agent-card.json",
                    SystemPrompt = section["SystemPrompt"],
                    AuthorizationBearerToken = section["AuthorizationBearerToken"],
                    ToolBindingMode = section["ToolBindingMode"],
                    RequirePublicUrl = ParseBool(section["RequirePublicUrl"], false),
                    PreferredBindings = preferredBindings,
                    AllowedSkills = allowedSkills
                });
            }

            return list;
        }

        private static bool ParseBool(string? value, bool defaultValue)
        {
            return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
        }
    }
}
