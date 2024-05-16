using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;

namespace Senaprc.AI.Samples.Agents.AgentUtility
{
    public class PrintWechatMessageMiddleware : IStreamingMiddleware, IMiddleware
    {
        public string? Name => "PrintWechatMessageMiddleware";

        public async Task<IMessage> InvokeAsync(MiddlewareContext context, IAgent agent, CancellationToken cancellationToken = default(CancellationToken))
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
                    Console.WriteLine(recentUpdate.FormatMessage());
                }

                return recentUpdate ?? throw new InvalidOperationException("The message is not a valid message");
            }

            IMessage obj = await agent.GenerateReplyAsync(context.Messages, context.Options, cancellationToken);
            Console.WriteLine(obj.FormatMessage());
            return obj;
        }

        public async IAsyncEnumerable<IStreamingMessage> InvokeAsync(MiddlewareContext context, IStreamingAgent agent, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
        {
            IMessage recentUpdate = null;
            await foreach (IStreamingMessage item in agent.GenerateStreamingReplyAsync(context.Messages, context.Options, cancellationToken))
            {
                if (item is TextMessageUpdate textMessageUpdate)
                {
                    if (recentUpdate == null)
                    {
                        Console.WriteLine("from: " + textMessageUpdate.From);
                        recentUpdate = new TextMessage(textMessageUpdate);
                        Console.Write(textMessageUpdate.Content);
                        yield return item;
                        continue;
                    }

                    if (!(recentUpdate is TextMessage textMessage))
                    {
                        throw new InvalidOperationException("The recent update is not a TextMessage");
                    }

                    Console.Write(textMessageUpdate.Content);
                    textMessage.Update(textMessageUpdate);
                    yield return textMessage;
                }
                else if (item is ToolCallMessageUpdate update)
                {
                    if (recentUpdate == null)
                    {
                        recentUpdate = new ToolCallMessage(update);
                        yield return item;
                        continue;
                    }

                    if (!(recentUpdate is ToolCallMessage toolCallMessage))
                    {
                        throw new InvalidOperationException("The recent update is not a ToolCallMessage");
                    }

                    toolCallMessage.Update(update);
                    yield return item;
                }
                else
                {
                    if (!(item is IMessage message))
                    {
                        throw new InvalidOperationException("The message is not a valid message");
                    }

                    recentUpdate = message;
                    yield return message;
                }
            }

            Console.WriteLine();
            if (recentUpdate != null && !(recentUpdate is TextMessage))
            {
                Console.WriteLine(recentUpdate.FormatMessage());
            }

            yield return recentUpdate ?? throw new InvalidOperationException("The message is not a valid message");
        }
    }
}
