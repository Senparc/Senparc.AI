using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.SemanticKernel;

namespace Senparc.AI.Agents.AgentExtensions;

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

    public static MiddlewareAgent<TAgent> RegisterCustomPrintMessage<TAgent, IPrintMessageMiddleware>(this MiddlewareAgent<TAgent> agent, IPrintMessageMiddleware printMessageMiddleware)
        where TAgent : IAgent
        where IPrintMessageMiddleware : IStreamingMiddleware, IMiddleware
    {
        //IPrintMessageMiddleware middleware = new IPrintMessageMiddleware();
        //printMessageMiddleware ??= new IPrintMessageMiddleware();

        MiddlewareAgent<TAgent> middlewareAgent = new MiddlewareAgent<TAgent>(agent);
        middlewareAgent.Use(printMessageMiddleware);
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

    #region AI Team 构造

    /// <summary>
    /// 创建 AITeam 对象（GroupChat）
    /// </summary>
    /// <typeparam name="TFromAgent"></typeparam>
    /// <param name="graphConnector"></param>
    /// <param name="adminAgent"></param>
    /// <returns></returns>
    public static GroupChat CreateAiTeam<TFromAgent>(this GraphConnector graphConnector, TFromAgent adminAgent) 
        where TFromAgent : IAgent
    {
        var aiTeam = new GroupChat(
            members: graphConnector.Agents.Values,
            admin: adminAgent,
            workflow: graphConnector.Graph);
        return aiTeam;
    }

    #endregion
}

