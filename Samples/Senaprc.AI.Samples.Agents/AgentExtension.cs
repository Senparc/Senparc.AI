using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.SemanticKernel;

namespace  Senparc.AI.Samples.Agents;

public static class AgentExtension
{
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
