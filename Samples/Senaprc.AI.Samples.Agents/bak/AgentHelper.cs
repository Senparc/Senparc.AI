using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senaprc.AI.Samples.Agents.Models;

namespace Senaprc.AI.Samples.Agents
{
    public static class AgentHelper
    {
        public static ConcurrentDictionary<string, Agent> AgentCollection = new ConcurrentDictionary<string, Agent>();


        public static void RegisterAllAgents()
        {
            AgentCollection["Guide"] = new Agent("1", "Guide", "Guide（导游）负责对所有任务进行分配", AgentState.None, "对所收到的任务进行规划，并分配到下一个 Agent");
            AgentCollection["Spider"] = new Agent("2", "Spider", "Spider（蜘蛛）负责对网络内容进行抓取", AgentState.None,"根据提供的[URL]信息进行网页内容爬取");
            AgentCollection["Writer"] = new Agent("3", "Writer", "Writer（写手）负责按照要求编写内容", AgentState.None, "理解[INPUT]的信息，并进行整理，输出预期的结果");
            AgentCollection["Executor"] = new Agent("4", "Executor", "Executor（执行者）负责按照要求执行系统指令", AgentState.None,"对[INPUT]提供的信息进行理解，并生成可靠的 CMD 命令行命令，注意：必须避免使用任何可能对文件进行删除的操作（移动文件除外）");

            string globalSystemRole = @"";
        }
    }
}
