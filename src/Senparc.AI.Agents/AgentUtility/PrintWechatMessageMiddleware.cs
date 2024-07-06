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

        public async Task<IMessage> InvokeAsync(MiddlewareContext context, IAgent agent, CancellationToken cancellationToken = default(CancellationToken))
        {
            // 检查 context 是否有消息，并且最新的消息是否是空字符串或仅包含空白字符
            var lastMessage = context.Messages?.LastOrDefault()?.GetContent();
            if (string.IsNullOrWhiteSpace(lastMessage))
            {
                // 如果是空的回车键或空白字符，继续程序的运行，不报异常错误
                Console.WriteLine("Received an empty or whitespace message. Continuing execution.");
                return null;
            }

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
