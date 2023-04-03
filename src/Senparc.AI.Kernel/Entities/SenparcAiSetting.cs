using Senparc.AI.Interfaces;

namespace Senparc.AI
{
    /// <inheritdoc/>
    public class SenparcAiSetting : ISenparcAiSetting
    {
        /// <inheritdoc/>
        public bool IsDebug { get; set; }

        /// <inheritdoc/>
        public bool UseAzureOpenAI => AiPlatform == AiPlatform.AzureOpenAI;

        /// <inheritdoc/>
        public AiPlatform AiPlatform { get; set; }

        /// <inheritdoc/>
        public string AzureEndpoint { get; set; }

        /// <inheritdoc/>
        public string ApiKey { get; set; }

        /// <inheritdoc/>
        public string OrgaizationId { get; set; }


        public SenparcAiSetting() { }
    }
}
