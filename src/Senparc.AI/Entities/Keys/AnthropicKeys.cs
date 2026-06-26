using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    /// <summary>
    /// Anthropic provider configuration.
    /// </summary>
    public class AnthropicKeys : BaseKeys
    {
        /// <summary>
        /// Anthropic API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Anthropic API endpoint. Default: https://api.anthropic.com
        /// </summary>
        public string Endpoint { get; set; } = "https://api.anthropic.com";

        /// <summary>
        /// Anthropic API version header value. Default: 2023-06-01.
        /// </summary>
        public string AnthropicVersion { get; set; } = "2023-06-01";
    }
}
