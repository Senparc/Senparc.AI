using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    /// <summary>
    /// FastAPI endpoint configuration (custom provider).
    /// </summary>
    public class FastAPIKeys : BaseKeys
    {
        /// <summary>
        /// FastAPI endpoint URL.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Optional API key for OpenAI-compatible FastAPI gateways.
        /// </summary>
        public string ApiKey { get; set; }
    }
}
