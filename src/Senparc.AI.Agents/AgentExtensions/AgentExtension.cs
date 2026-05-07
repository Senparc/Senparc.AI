using AutoGen.Core;
using AutoGen.SemanticKernel;

namespace Senparc.AI.Agents.AgentExtensions;

public static class AgentExtension
{
    public static MiddlewareAgent<SemanticKernelAgent> RegisterTextMessageConnector(this SemanticKernelAgent agent, SemanticKernelChatMessageContentConnector? connector = null)
    {
        connector ??= new SemanticKernelChatMessageContentConnector();
        return agent.RegisterMiddleware(connector);
    }

    public static MiddlewareAgent<TAgent> RegisterCustomPrintMessage<TAgent, IPrintMessageMiddleware>(this MiddlewareAgent<TAgent> agent, IPrintMessageMiddleware printMessageMiddleware)
        where TAgent : IAgent
        where IPrintMessageMiddleware : IStreamingMiddleware, IMiddleware
    {
        var middlewareAgent = new MiddlewareAgent<TAgent>(agent.Agent);
        middlewareAgent.Use(printMessageMiddleware);
        return middlewareAgent;
    }

    public static GraphConnector ConnectFrom<TFromAgent>(this Graph graph, TFromAgent fromAgent) where TFromAgent : IAgent
    {
        var fromGraph = new GraphConnector(graph, fromAgent);
        fromGraph.AddAgent(fromAgent);
        return fromGraph;
    }

    public static GraphConnector ConnectFrom<TFromAgent>(this GraphConnectorEnd graphEnd, TFromAgent fromAgent) where TFromAgent : IAgent
    {
        var fromGraph = graphEnd.GraphConnector;
        var newFromGraph = fromGraph with { FromAgent = fromAgent };
        newFromGraph.AddAgent(fromAgent);
        return newFromGraph;
    }

    public static GraphConnectorEnd To<TToAgent>(this GraphConnector fromGraph, TToAgent toAgent) where TToAgent : IAgent
    {
        fromGraph.Graph.AddTransition(Transition.Create(fromGraph.FromAgent, toAgent));
        fromGraph.AddAgent(toAgent);
        return new GraphConnectorEnd(fromGraph);
    }

    public static GraphConnectorEnd AlsoTo<TToAgent>(this GraphConnectorEnd fromGraph, TToAgent toAgent) where TToAgent : IAgent
    {
        return To(fromGraph.GraphConnector, toAgent);
    }

    public static GraphConnectorEnd TwoWay<TToAgent>(this GraphConnector fromGraph, TToAgent to) where TToAgent : IAgent
    {
        fromGraph.Graph.AddTransition(Transition.Create(fromGraph.FromAgent, to));
        fromGraph.Graph.AddTransition(Transition.Create(to, fromGraph.FromAgent));
        fromGraph.AddAgent(to);
        return new GraphConnectorEnd(fromGraph);
    }

    public static GraphConnectorEnd AlsoTwoWay<TToAgent>(this GraphConnectorEnd fromGraph, TToAgent to) where TToAgent : IAgent
    {
        return TwoWay(fromGraph.GraphConnector, to);
    }

    public static GroupChat CreateAiTeam<TFromAgent>(this GraphConnector graphConnector, TFromAgent adminAgent) where TFromAgent : IAgent
    {
        return new GroupChat(graphConnector.Agents.Values, adminAgent, graphConnector.Graph);
    }

    [Obsolete("请使用方法 CreateAiTeam<TFromAgent>(this GraphConnector graphConnector, IOrchestrator orchestrator)")]
    public static GroupChat CreateAiTeam<TFromAgent>(this GraphConnector graphConnector, TFromAgent adminAgent, IOrchestrator orchestrator) where TFromAgent : IAgent
    {
        return new GroupChat(graphConnector.Agents.Values, adminAgent, graphConnector.Graph, orchestrator);
    }

    public static GroupChat CreateAiTeam<TFromAgent>(this GraphConnector graphConnector, IOrchestrator orchestrator) where TFromAgent : IAgent
    {
        return new GroupChat(graphConnector.Agents.Values, workflow: graphConnector.Graph, orchestrator: orchestrator);
    }
}
