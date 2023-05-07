using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
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
