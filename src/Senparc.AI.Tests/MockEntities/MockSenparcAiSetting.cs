using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Tests.MockEntities
{
    public class MockSenparcAiSetting:ISenparcAiSetting
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
        public string AzureEndpoint { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string OrgaizationId { get; set; }
    }
}
