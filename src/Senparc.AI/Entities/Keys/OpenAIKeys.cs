using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    public class OpenAIKeys : BaseKeys
    {
        public string ApiKey { get; set; }
        public string OrganizationId { get; set; }

        [JsonProperty(
            NullValueHandling = NullValueHandling.Ignore, 
            DefaultValueHandling =DefaultValueHandling.Ignore)]
        public string? OpenAIEndpoint { get; set; }
    }
}
