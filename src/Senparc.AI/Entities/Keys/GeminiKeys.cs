using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    /// <summary>
    /// Gemini provider configuration.
    /// </summary>
    public class GeminiKeys : BaseKeys
    {
        /// <summary>
        /// Gemini API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gemini API endpoint. Default: https://generativelanguage.googleapis.com
        /// </summary>
        public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com";

        /// <summary>
        /// Whether to use Vertex AI mode.
        /// </summary>
        public bool UseVertexAI { get; set; }

        /// <summary>
        /// Vertex AI project id.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Vertex AI location.
        /// </summary>
        public string Location { get; set; } = "global";
    }
}
