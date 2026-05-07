namespace AutoGen.Core
{
    public interface IMessage
    {
        string From { get; }
        string Content { get; }
        string FormatMessage();
    }

    public class TextMessage : IMessage
    {
        public string From { get; set; } = "assistant";
        public string Content { get; set; } = string.Empty;

        public TextMessage() { }

        public TextMessage(string from, string content)
        {
            From = from;
            Content = content;
        }

        public string FormatMessage() => $"{From}: {Content}";
    }

    public class MiddlewareContext
    {
        public IReadOnlyList<IMessage> Messages { get; set; } = [];
        public object? Options { get; set; }
    }

    public interface IAgent
    {
        string Name { get; }
        Task<IMessage> GenerateReplyAsync(IReadOnlyList<IMessage> messages, object? options = null, CancellationToken cancellationToken = default);
    }

    public interface IStreamingAgent : IAgent
    {
        IAsyncEnumerable<IMessage> GenerateStreamingReplyAsync(IReadOnlyList<IMessage> messages, object? options = null, CancellationToken cancellationToken = default);
    }

    public interface IMiddleware
    {
        Task<IMessage> InvokeAsync(MiddlewareContext context, IAgent agent, CancellationToken cancellationToken = default);
    }

    public interface IStreamingMiddleware
    {
        IAsyncEnumerable<IMessage> InvokeAsync(MiddlewareContext context, IStreamingAgent agent, CancellationToken cancellationToken = default);
    }

    public class MiddlewareAgent<TAgent> where TAgent : IAgent
    {
        public TAgent Agent { get; }
        public List<object> Middlewares { get; } = [];

        public MiddlewareAgent(TAgent agent)
        {
            Agent = agent;
        }

        public MiddlewareAgent<TAgent> Use(object middleware)
        {
            Middlewares.Add(middleware);
            return this;
        }
    }

    public class Transition
    {
        public IAgent From { get; set; } = null!;
        public IAgent To { get; set; } = null!;

        public static Transition Create(IAgent from, IAgent to) => new() { From = from, To = to };
    }

    public class Graph
    {
        public List<Transition> Transitions { get; } = [];

        public Graph(IEnumerable<Transition>? transitions = null)
        {
            if (transitions is not null)
            {
                Transitions.AddRange(transitions);
            }
        }

        public void AddTransition(Transition transition) => Transitions.Add(transition);
    }

    public interface IOrchestrator { }

    public class GroupChat
    {
        public IReadOnlyCollection<IAgent> Members { get; }
        public IAgent? Admin { get; }
        public Graph? Workflow { get; }
        public IOrchestrator? Orchestrator { get; }

        public GroupChat(IEnumerable<IAgent> members, IAgent? admin = null, Graph? workflow = null, IOrchestrator? orchestrator = null)
        {
            Members = members.ToList().AsReadOnly();
            Admin = admin;
            Workflow = workflow;
            Orchestrator = orchestrator;
        }
    }
}

namespace AutoGen.SemanticKernel
{
    using AutoGen.Core;

    public class SemanticKernelChatMessageContentConnector : IStreamingMiddleware, IMiddleware
    {
        public Task<IMessage> InvokeAsync(MiddlewareContext context, IAgent agent, CancellationToken cancellationToken = default)
        {
            return agent.GenerateReplyAsync(context.Messages, context.Options, cancellationToken);
        }

        public async IAsyncEnumerable<IMessage> InvokeAsync(MiddlewareContext context, IStreamingAgent agent, CancellationToken cancellationToken = default)
        {
            await foreach (var message in agent.GenerateStreamingReplyAsync(context.Messages, context.Options, cancellationToken))
            {
                yield return message;
            }
        }
    }

    public class SemanticKernelAgent : IStreamingAgent
    {
        public string Name { get; }

        public SemanticKernelAgent(string name)
        {
            Name = name;
        }

        public Task<IMessage> GenerateReplyAsync(IReadOnlyList<IMessage> messages, object? options = null, CancellationToken cancellationToken = default)
        {
            var content = messages.LastOrDefault()?.Content ?? string.Empty;
            return Task.FromResult<IMessage>(new TextMessage(Name, $"[MAF-Compat] {content}"));
        }

        public async IAsyncEnumerable<IMessage> GenerateStreamingReplyAsync(IReadOnlyList<IMessage> messages, object? options = null, CancellationToken cancellationToken = default)
        {
            var reply = await GenerateReplyAsync(messages, options, cancellationToken);
            yield return reply;
        }

        public MiddlewareAgent<SemanticKernelAgent> RegisterMiddleware(object middleware)
        {
            return new MiddlewareAgent<SemanticKernelAgent>(this).Use(middleware);
        }
    }
}
