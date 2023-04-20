using Senparc.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Tests.MockEntities
{
    public class MockSenparcAiSetting : ISenparcAiSetting
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public AiPlatform AiPlatform { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public AzureOpenAIKeys AzureOpenAIKeys { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public OpenAIKeys OpenAIKeys { get; set; }
    }
}
