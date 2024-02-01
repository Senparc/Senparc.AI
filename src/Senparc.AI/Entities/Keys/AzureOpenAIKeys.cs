using System;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    public class AzureOpenAIKeys: BaseKeys, IDeployment
    {
        public string ApiKey { get; set; }
        public string AzureEndpoint { get; set; }
        /// <summary>
        /// AzureOpenAIApiVersion
        /// 调用限制请参考：https://learn.microsoft.com/en-us/azure/cognitive-services/openai/quotas-limits
        /// </summary>
        public string AzureOpenAIApiVersion { get; set; } = "2022-12-01";
        public string DeploymentName { get; set; }
    }
}
