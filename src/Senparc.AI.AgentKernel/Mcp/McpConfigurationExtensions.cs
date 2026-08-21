using Microsoft.Extensions.Configuration;
using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Senparc.AI.AgentKernel.Mcp
{
    /// <summary>
    /// MCP configuration extensions.
    /// </summary>
    public static class McpConfigurationExtensions
    {
        /// <summary>
        /// Read the MCP Server list from configuration.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="sectionPath">Default: SenparcAiSetting:McpServers</param>
        /// <returns></returns>
        public static IReadOnlyList<McpServerOption> GetMcpServerOptions(this IConfiguration configuration, string sectionPath = "SenparcAiSetting:McpServers")
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var list = new List<McpServerOption>();
            var serverSections = configuration.GetSection(sectionPath).GetChildren();

            foreach (var section in serverSections)
            {
                var name = section["Name"] ?? section["ServerName"] ?? "McpServer";
                var serverName = section["ServerName"] ?? name;
                var sseUrl = section["SseUrl"];
                var localSseUrl = section["LocalSseUrl"];

                var allowedTools = section.GetSection("AllowedTools")
                    .GetChildren()
                    .Select(z => z.Value?.Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .Cast<string>()
                    .ToList();

                if (string.IsNullOrWhiteSpace(serverName) ||
                    (string.IsNullOrWhiteSpace(sseUrl) && string.IsNullOrWhiteSpace(localSseUrl)))
                {
                    continue;
                }

                list.Add(new McpServerOption
                {
                    Name = name,
                    ServerName = serverName,
                    SseUrl = sseUrl,
                    LocalSseUrl = localSseUrl,
                    PublicBaseUrl = section["PublicBaseUrl"],
                    ServerDescription = section["ServerDescription"],
                    SystemPrompt = section["SystemPrompt"],
                    AuthorizationBearerToken = section["AuthorizationBearerToken"],
                    RequirePublicUrl = ParseBool(section["RequirePublicUrl"], true),
                    ToolBindingMode = section["ToolBindingMode"],
                    AllowedTools = allowedTools
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
