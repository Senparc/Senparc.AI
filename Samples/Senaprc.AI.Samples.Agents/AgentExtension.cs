using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.SemanticKernel;
using Senaprc.AI.Samples.Agents.AgentUtility;

namespace Senparc.AI.Samples.Agents;

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

    //public static MiddlewareStreamingAgent<OpenAIChatAgent> RegisterMessageConnector(this OpenAIChatAgent agent)
    //{
    //    var messageConnector = new OpenAIChatRequestMessageConnector();
    //    return agent.RegisterStreamingMiddleware(messageConnector)
    //        .RegisterMiddleware(messageConnector);
    //}

    //public static MiddlewareStreamingAgent<SemanticKernelAgent> RegisterMessageConnector(this SemanticKernelAgent agent)
    //{
    //    var messageConnector = new SemanticKernelChatMessageContentConnector();
    //    return agent.RegisterStreamingMiddleware(messageConnector)
    //        .RegisterMiddleware(messageConnector);
    //}
}
