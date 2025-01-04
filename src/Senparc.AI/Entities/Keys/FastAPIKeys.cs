using System;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.Entities.Keys;

namespace Senparc.AI
{
    public class FastAPIKeys : BaseKeys
    {
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public string OrganizationId { get; set; }
    }
}
