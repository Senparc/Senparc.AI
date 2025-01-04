using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace Senaprc.AI.Agents.AgentExtensions
{
    public static class GraphBuilder
    {
        public static Graph Start(IEnumerable<Transition>? transitions = null)
        {
            //TODO: 这里应该提供不带参数的构造函数，已经提交 PR，等待合并
            var graph = new Graph(transitions ?? new List<Transition>());
            return graph;
        }
    }
}
