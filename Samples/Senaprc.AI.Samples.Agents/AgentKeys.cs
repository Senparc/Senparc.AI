using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using Senparc.CO2NET.Trace;

namespace Senparc.AI.Agents
{
    public static class AgentKeys
    {
        public static string AgentKey1 = null;
        public static string AgentKey2 = null;
        public static string AgentKey3 = null;

        public static Action<IAgent, IMessage, string> SendWechatMessage => async (agent, replyObject, message) =>
         {
             string key = null;
             switch (agent.Name)
             {
                 case "Administrative Manager":
                     key = AgentKeys.AgentKey1;
                     break;
                 case "Product Manager":
                     key = AgentKeys.AgentKey2;
                     break;
                 case "Project Manager":
                     key = AgentKeys.AgentKey3;
                     break;
                 default:
                     break;
             }

             if (key != null)
             {
                 try
                 {
                     await Senparc.Weixin.Work.AdvancedAPIs.Webhook.WebhookApi.SendTextAsync(key, message);
                 }
                 catch (Exception ex)
                 {
                     SenparcTrace.BaseExceptionLog(ex);
                 }
             }
         };
    }
}
