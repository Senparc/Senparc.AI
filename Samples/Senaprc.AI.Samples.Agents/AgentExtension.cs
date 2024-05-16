using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.SemanticKernel;

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
        PrintMessageMiddleware middleware = new PrintMessageMiddleware();
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
