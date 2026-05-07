using AutoGen.Core;
using Senparc.CO2NET.Trace;

namespace Senparc.AI.Agents.AgentUtility;

public class PrintWechatMessageMiddleware : IStreamingMiddleware, IMiddleware
{
    private readonly Action<IAgent, IMessage, string>? _sendMessageAction;

    public string? Name => "PrintWechatMessageMiddleware";

    public PrintWechatMessageMiddleware(Action<IAgent, IMessage, string>? sendMessageAction)
    {
        _sendMessageAction = sendMessageAction;
    }

    public async Task<IMessage> InvokeAsync(MiddlewareContext context, IAgent agent, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await agent.GenerateReplyAsync(context.Messages, context.Options, cancellationToken);
            var output = result.FormatMessage();
            _sendMessageAction?.Invoke(agent, result, output);
            Console.WriteLine(output);
            return result;
        }
        catch (Exception ex)
        {
            SenparcTrace.SendCustomLog("PrintWechatMessageMiddleware 异常", ex.ToString());
            SenparcTrace.BaseExceptionLog(ex);
            throw;
        }
    }

    public async IAsyncEnumerable<IMessage> InvokeAsync(MiddlewareContext context, IStreamingAgent agent, CancellationToken cancellationToken = default)
    {
        await foreach (var message in agent.GenerateStreamingReplyAsync(context.Messages, context.Options, cancellationToken))
        {
            Console.WriteLine(message.FormatMessage());
            yield return message;
        }
    }
}
