using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senaprc.AI.Samples.Agents.Models
{
    public class Agent
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SystemRole { get; set; }
        public AgentState AgentState { get; set; }
        public List<AgentHistory> AgentHistories { get; set; } = new List<AgentHistory>();
        public List<WorldKnowledge> WorldKnowledges { get; set; }

        public Func<Dictionary<string, string>, Dictionary<string, string>> ExecutionAction { get; set; }

        public Agent(string uid, string name, string description, AgentState agentState, string systemRole, Func<Dictionary<string, string>, Dictionary<string, string>> executionAction = null)
        {
            Uid = uid;
            Name = name;
            Description = description;
            AgentState = agentState;
            SystemRole = systemRole;
            ExecutionAction = executionAction;
        }

        public Dictionary<string, string> Execute(Dictionary<string, string> args)
        {
            //TODO 加入当前Agent的上下文判断和AI

            return ExecutionAction?.Invoke(args) ?? new Dictionary<string, string>();
        }
    }
}
