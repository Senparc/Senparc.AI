using System;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    public class OpenAIKeys : BaseKeys
    {
        public string ApiKey { get; set; }
        public string OrganizationId { get; set; }
        public string OpenAIProxyEndpoint {  get; set; }
    }
}
