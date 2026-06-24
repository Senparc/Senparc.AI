using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    /// <summary>
    /// Kimi (Moonshot/OpenAI-compatible endpoint) configuration.
    /// </summary>
    public class KimiKeys : BaseKeys
    {
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }
    }
}
