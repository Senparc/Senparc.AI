using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    public class AzureOpenAIKeys
    {
        public string ApiKey { get; set; }
        public string AzureEndpoint { get; set; }
        /// <summary>
        /// AzureOpenAIApiVersion
        /// 调用限制请参考：https://learn.microsoft.com/en-us/azure/cognitive-services/openai/quotas-limits
        /// </summary>
        public string AzureOpenAIApiVersion { get; set; } = "2022-12-01";
    }

    public class NeuCharOpenAIKeys
    {
        public string ApiKey { get; set; }
        public string NeuCharEndpoint { get; set; }
        /// <summary>
        /// NeuCharOpenAIApiVersion，固定值
        /// </summary>
        public string NeuCharOpenAIApiVersion { get; set; } = "2022-12-01";
    }
}
