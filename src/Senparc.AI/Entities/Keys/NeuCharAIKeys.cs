using System;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    [Obsolete("已过期，请使用 NeuCharAIKeys", true)]
    public class NeuCharOpenAIAIKeys
    {
        public string ApiKey { get; set; }
        public string NeuCharEndpoint { get; set; }
        /// <summary>
        /// NeuCharOpenAIApiVersion，固定值
        /// </summary>
        public string NeuCharOpenAIApiVersion { get; set; } = "2022-12-01";
    }

    public class NeuCharAIKeys : BaseKeys
    {
        public string ApiKey { get; set; }
        public string NeuCharEndpoint { get; set; }
        /// <summary>
        /// NeuCharOpenAIApiVersion，固定值
        /// </summary>
        public string NeuCharAIApiVersion { get; set; } = "2022-12-01";
    }
}
