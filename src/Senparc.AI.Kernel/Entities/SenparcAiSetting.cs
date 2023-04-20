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

        public AzureOpenAIKeys AzureOpenAIKeys { get; set; }
        public OpenAIKeys OpenAIKeys { get; set; }

        /// <summary>
        /// Azure OpenAI 或 OpenAI API Key
        /// </summary>
        string ApiKey => AiPlatform switch
        {
            AiPlatform.AzureOpenAI => AzureOpenAIKeys.ApiKey,
            AiPlatform.OpenAI => OpenAIKeys.ApiKey,
        };

        public SenparcAiSetting() { }
    }
}
