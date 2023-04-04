using Senparc.AI.Interfaces;

namespace Senparc.AI
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class SenparcAiSetting : ISenparcAiSetting
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>

        public bool IsDebug { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseAzureOpenAI => AiPlatform == AiPlatform.AzureOpenAI;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public AiPlatform AiPlatform { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string OrgaizationId { get; set; }

        #region Azure OpenAI

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string AzureEndpoint { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string AzureOpenAIApiVersion { get; set; }

        #endregion


        public SenparcAiSetting() { }
    }
}
