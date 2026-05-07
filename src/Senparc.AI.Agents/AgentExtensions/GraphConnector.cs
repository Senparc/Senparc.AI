using AutoGen.Core;

namespace Senparc.AI.Agents.AgentExtensions;

public record class GraphConnector
{
    public Graph Graph { get; set; }
    public IAgent FromAgent { get; set; } = null!;
    public IAgent? AdminAgent { get; set; }
    public Dictionary<string, IAgent> Agents { get; set; } = [];

    public GraphConnector()
    {
        Graph = new Graph([]);
    }

    public GraphConnector(Graph graph, IAgent fromAgent)
    {
        Graph = graph;
        FromAgent = fromAgent;
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

    public GraphConnector Finish() => GraphConnector;
}
