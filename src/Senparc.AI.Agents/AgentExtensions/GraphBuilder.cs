using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace Senparc.AI.Agents.AgentExtensions
{
    public static class GraphBuilder
    {
        public static Graph Start(IEnumerable<Transition>? transitions = null)
        {
            //TODO: A parameterless constructor should be provided here. A PR has already been submitted and is waiting to be merged.
            var graph = new Graph(transitions ?? new List<Transition>());
            return graph;
        }
    }
}
