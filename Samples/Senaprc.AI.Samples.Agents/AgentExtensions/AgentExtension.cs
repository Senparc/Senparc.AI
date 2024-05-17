using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.SemanticKernel;
using Senaprc.AI.Samples.Agents.AgentUtility;

namespace Senaprc.AI.Samples.Agents.AgentExtensions;

public static class AgentExtension
{

    public static MiddlewareAgent<SemanticKernelAgent> RegisterTextMessageConnector(this SemanticKernelAgent agent, SemanticKernelChatMessageContentConnector? connector = null)
    {
        if (connector == null)
        {
            connector = new SemanticKernelChatMessageContentConnector();
        }

        return agent.RegisterMiddleware(connector);
    }

    public static MiddlewareAgent<TAgent> RegisterPrintWechatMessage<TAgent>(this MiddlewareAgent<TAgent> agent) where TAgent : IAgent
    {
        PrintWechatMessageMiddleware middleware = new PrintWechatMessageMiddleware();
        MiddlewareAgent<TAgent> middlewareAgent = new MiddlewareAgent<TAgent>(agent);
        middlewareAgent.Use(middleware);
        return middlewareAgent;
    }

    #region 快速构建 Graph 扩展方法

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
        fromGraph.AddAgent(fromAgent);
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


    #endregion

}

