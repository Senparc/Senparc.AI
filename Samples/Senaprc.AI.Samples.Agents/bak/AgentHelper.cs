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
            AgentCollection["Guide"] = new Agent("1", "Guide", "Guide(Guide)responsible for assigning all tasks", AgentState.None, "Plan received tasks and assign them to the next Agent");
            AgentCollection["Spider"] = new Agent("2", "Spider", "Spider(Spider)responsible for crawling web content", AgentState.None,"Crawl web content based on the provided [URL]");
            AgentCollection["Writer"] = new Agent("3", "Writer", "Writer(Writer)responsible for writing content as requested", AgentState.None, "Understand the information in [INPUT], organize it, and output the expected result");
            AgentCollection["Executor"] = new Agent("4", "Executor", "Executor(Executor)responsible for executing system commands as requested", AgentState.None,"Understand the information provided by [INPUT] and generate reliable CMD commands. Note: avoid any operation that may delete files, except moving files.");

            string globalSystemRole = @"";
        }
    }
}
