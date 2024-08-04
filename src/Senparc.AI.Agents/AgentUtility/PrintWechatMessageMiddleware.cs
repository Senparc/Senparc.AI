using AutoGen.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Senaprc.AI.Agents.AgentUtility
{
    public class PrintWechatMessageMiddleware : IStreamingMiddleware, IMiddleware
    {
        private readonly Action<IAgent, IMessage, string>? _sendMessageAction;

        public string? Name => "PrintWechatMessageMiddleware";


        public PrintWechatMessageMiddleware(Action<IAgent, IMessage, string>? sendMessageAction)
        {
            this._sendMessageAction = sendMessageAction;
        }

        public async Task<IMessage> InvokeAsync(MiddlewareContext context, IAgent agent, CancellationToken cancellationToken = default)
        {
            if (agent is IStreamingAgent agent2)
            {
                IMessage recentUpdate = null;
                await foreach (IStreamingMessage item in InvokeAsync(context, agent2, cancellationToken))
                {
                    if (item is IMessage message)
                    {
                        recentUpdate = message;
                    }
                }

                Console.WriteLine();
                if (recentUpdate != null && !(recentUpdate is TextMessage))
                {
                    Console.WriteLine("STREAM: " + recentUpdate.FormatMessage());
                }

                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("====StreamAgent====");
                await Console.Out.WriteLineAsync();

                return recentUpdate ?? throw new InvalidOperationException("The message is not a valid message");
            }

            IMessage obj = await agent.GenerateReplyAsync(context.Messages, context.Options, cancellationToken);
            var outputMessage = obj.FormatMessage();
            _sendMessageAction?.Invoke(agent, obj, outputMessage);
            Console.WriteLine(obj.FormatMessage());
            return obj;
        }

        public async IAsyncEnumerable<IMessage> InvokeAsync(MiddlewareContext context, IStreamingAgent agent, CancellationToken cancellationToken)
        {
            IMessage? recentUpdate = null;
            await foreach (var message in agent.GenerateStreamingReplyAsync(context.Messages, context.Options, cancellationToken))
            {
                if (message is TextMessageUpdate textMessageUpdate)
                {
                    if (recentUpdate is null)
                    {
                        // Print from: xxx
                        Console.WriteLine($"from: {textMessageUpdate.From}");
                        recentUpdate = new TextMessage(textMessageUpdate);
                        Console.Write(textMessageUpdate.Content);

                        yield return message;
                    }
                    else if (recentUpdate is TextMessage recentTextMessage)
                    {
                        // Print the content of the message
                        Console.Write(textMessageUpdate.Content);
                        recentTextMessage.Update(textMessageUpdate);

                        yield return recentTextMessage;
                    }
                    else
                    {
                        throw new InvalidOperationException("The recent update is not a TextMessage");
                    }
                }
                else if (message is ToolCallMessageUpdate toolCallUpdate)
                {
                    if (recentUpdate is null)
                    {
                        recentUpdate = new ToolCallMessage(toolCallUpdate);

                        yield return message;
                    }
                    else if (recentUpdate is ToolCallMessage recentToolCallMessage)
                    {
                        recentToolCallMessage.Update(toolCallUpdate);

                        yield return message;
                    }
                    else
                    {
                        throw new InvalidOperationException("The recent update is not a ToolCallMessage");
                    }
                }
                else if (message is IMessage imessage)
                {
                    recentUpdate = imessage;

                    yield return imessage;
                }
                else
                {
                    throw new InvalidOperationException("The message is not a valid message");
                }
            }
            Console.WriteLine();
            if (recentUpdate is not null && recentUpdate is not TextMessage)
            {
                Console.WriteLine(recentUpdate.FormatMessage());
            }

            yield return recentUpdate ?? throw new InvalidOperationException("The message is not a valid message");
        }
    }
}
