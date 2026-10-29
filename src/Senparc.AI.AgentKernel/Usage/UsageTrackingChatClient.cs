using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.Usage;

internal sealed class UsageTrackingChatClient : IChatClient
{
    private readonly IChatClient _innerClient;
    private readonly IAIUsageObserver _observer;
    private readonly AIUsageContext _context;

    private UsageTrackingChatClient(IChatClient innerClient, IAIUsageObserver observer, AIUsageContext context)
    {
        _innerClient = innerClient;
        _observer = observer;
        _context = context;
    }

    public static IChatClient Wrap(IChatClient innerClient, IAIUsageObserver observer, AIUsageContext context)
    {
        ArgumentNullException.ThrowIfNull(innerClient);
        ArgumentNullException.ThrowIfNull(observer);
        return new UsageTrackingChatClient(innerClient, observer, context);
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            var response = await _innerClient.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            Dispatch(new AIUsageEvent
            {
                Operation = "chat.response",
                Context = _context,
                Source = _context.Source,
                Model = _context.ModelName,
                Usage = response.Usage,
                Success = true,
                StartedAt = startedAt,
                CompletedAt = DateTimeOffset.UtcNow
            });
            return response;
        }
        catch (Exception exception)
        {
            Dispatch(new AIUsageEvent
            {
                Operation = "chat.response",
                Context = _context,
                Source = _context.Source,
                Model = _context.ModelName,
                Success = false,
                StartedAt = startedAt,
                CompletedAt = DateTimeOffset.UtcNow,
                Exception = exception
            });
            throw;
        }
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        UsageDetails? usage = null;
        Exception? exception = null;

        await using var enumerator = _innerClient
            .GetStreamingResponseAsync(messages, options, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        while (true)
        {
            bool hasNext;
            try
            {
                hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
            }
            catch (Exception caughtException)
            {
                exception = caughtException;
                break;
            }

            if (!hasNext)
            {
                break;
            }

            var update = enumerator.Current;
            if (update.Contents is not null)
            {
                foreach (var content in update.Contents)
                {
                    if (content is UsageContent usageContent)
                    {
                        usage = usageContent.Details ?? usage;
                    }
                }
            }

            yield return update;
        }

        Dispatch(new AIUsageEvent
        {
            Operation = "chat.streaming",
            Context = _context,
            Source = _context.Source,
            Model = _context.ModelName,
            Usage = usage,
            Success = exception is null,
            StartedAt = startedAt,
            CompletedAt = DateTimeOffset.UtcNow,
            Exception = exception
        });

        if (exception is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType == typeof(IChatClient) || serviceType == typeof(UsageTrackingChatClient)
            ? this
            : _innerClient.GetService(serviceType, serviceKey);
    }

    public void Dispose() => _innerClient.Dispose();

    private void Dispatch(AIUsageEvent usageEvent)
    {
        ThreadPool.QueueUserWorkItem(
            static state =>
            {
                var item = (ObserverWorkItem)state!;
                try
                {
                    item.Observer.Observe(item.Event);
                }
                catch
                {
                }
            },
            new ObserverWorkItem(_observer, usageEvent));
    }

    private sealed record ObserverWorkItem(IAIUsageObserver Observer, AIUsageEvent Event);
}
