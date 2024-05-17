using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace Senaprc.AI.Samples.Agents.AgentExtensions
{

    public record class GraphConnector
    {
        public Graph Graph { get; set; }
        public IAgent FromAgent { get; set; }

        public Dictionary<string, IAgent> Agents { get; set; }

        public GraphConnector()
        {
            Graph = new Graph(new List<Transition>());
        }

        public GraphConnector(Graph graph, IAgent fromAgent)
        {
            Graph = graph;
            FromAgent = fromAgent;
            Agents = new Dictionary<string, IAgent>();

            AddAgent(fromAgent);
        }

        public void AddAgent(IAgent agent)
        {
            Agents[agent.Name] = agent;
        }
    }

    public record class GraphConnectorEnd
    {
        public GraphConnector GraphConnector { get; set; }

        public GraphConnectorEnd(GraphConnector graphConnector)
        {
            GraphConnector = graphConnector;
        }

        public GraphConnector Finish()
        {
            return GraphConnector;
        }
    }
}
