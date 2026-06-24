using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    /// <summary>
    /// Qwen (DashScope/OpenAI-compatible endpoint) configuration.
    /// </summary>
    public class QwenKeys : BaseKeys
    {
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }
    }
}
