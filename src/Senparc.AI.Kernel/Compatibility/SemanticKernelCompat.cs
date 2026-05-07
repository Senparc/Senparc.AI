using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class KernelFunctionAttribute : Attribute
    {
        public KernelFunctionAttribute() { }
        public KernelFunctionAttribute(string name) { }
    }

    public class KernelPluginCollection
    {
        public KernelPluginCollection AddFromType<T>() => this;
        public KernelPluginCollection AddFromObject(object plugin) => this;
    }

    public interface IAIService { }

    public interface IKernelBuilder
    {
        IServiceCollection Services { get; }
        KernelPluginCollection Plugins { get; }
    }

    public class KernelBuilder : IKernelBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
        public KernelPluginCollection Plugins { get; } = new();
    }

    public class Kernel
    {
        public static IKernelBuilder CreateBuilder() => new KernelBuilder();
        public KernelPluginCollection Plugins { get; } = new();
    }

    public class KernelArguments : Dictionary<string, object?>
    {
        public void Set(string key, object? value) => this[key] = value;
    }

    public class KernelFunction
    {
        public string Name { get; set; } = "CompatFunction";
    }

    public class FunctionResult
    {
        public object? Value { get; set; }

        public T? GetValue<T>()
        {
            if (Value is T typed)
            {
                return typed;
            }
            if (Value is null)
            {
                return default;
            }
            return (T?)Convert.ChangeType(Value, typeof(T));
        }
    }

    public class FunctionResultContent
    {
        public string? FunctionName { get; set; }
        public string? Result { get; set; }
    }

    public class StreamingKernelContent : ChatCompletion.StreamingKernelContent
    {
    }
}

namespace Microsoft.SemanticKernel.Services
{
    public interface IAIService { }
}

namespace Microsoft.SemanticKernel.ChatCompletion
{
    public enum AuthorRole
    {
        System = 0,
        User = 1,
        Assistant = 2
    }

    public class StreamingKernelContent
    {
        public string Content { get; set; } = string.Empty;
        public override string ToString() => Content;
    }

    public class ChatMessageContent
    {
        public AuthorRole Role { get; set; }
        public string Content { get; set; } = string.Empty;

        public ChatMessageContent(AuthorRole role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    public class ChatHistory : List<ChatMessageContent>
    {
        public void AddUserMessage(string content) => Add(new ChatMessageContent(AuthorRole.User, content));
        public void AddAssistantMessage(string content) => Add(new ChatMessageContent(AuthorRole.Assistant, content));
        public void AddSystemMessage(string content) => Add(new ChatMessageContent(AuthorRole.System, content));
    }
}
