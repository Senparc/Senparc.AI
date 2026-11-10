/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    File: AgentKernelHarness.cs
    Description: Native Microsoft Agent Framework Harness integration for AgentKernel.

    Created by: Senparc - 20260911

    Modified by: Senparc - 20260916
    Description: v0.1.14-preview4 require native Harness streaming transport

    Modified by: Senparc - 20261111
    Description: v0.1.16 adapt Microsoft Agent Framework Harness 1.28

    Modified by: Senparc - 20261111
    Description: Keep Harness compaction from dropping a user turn when the todo status text repeats

----------------------------------------------------------------*/

#pragma warning disable MAAI001

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.Interfaces;
using Senparc.AI.AgentKernel.Kernels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MafChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Senparc.AI.AgentKernel.Harness;

/// <summary>
/// AgentKernel-friendly options for creating a native Microsoft Agent Framework Harness agent.
/// </summary>
public sealed class AgentKernelHarnessOptions
{
    /// <summary>
    /// Maximum model context window used by MAF compaction.
    /// </summary>
    public int MaxContextWindowTokens { get; init; } = 32_768;

    /// <summary>
    /// Maximum model output tokens per response.
    /// </summary>
    public int MaxOutputTokens { get; init; } = 2_048;

    /// <summary>
    /// Optional native MAF Harness options. When omitted, least-privilege defaults are created.
    /// </summary>
    public HarnessAgentOptions? HarnessOptions { get; init; }

    /// <summary>
    /// Creates the default least-privilege MAF options for an AgentKernel application.
    /// </summary>
    public static HarnessAgentOptions CreateLeastPrivilegeOptions(
        string name = "SenparcAgentKernelHarness",
        string description = "Senparc.AI AgentKernel Harness agent")
    {
        return new HarnessAgentOptions
        {
            Name = name,
            Description = description,
            // MAF 1.14+ treats file access as opt-in through FileAccessStore.
            DisableFileMemory = true,
            DisableAgentSkillsProvider = true,
            DisableWebSearch = true
        };
    }
}

/// <summary>
/// A native MAF Harness agent and its current session.
/// </summary>
public sealed class AgentKernelHarness
{
    internal AgentKernelHarness(AIAgent agent, AgentSession session)
    {
        Agent = agent;
        Session = session;
    }

    /// <summary>
    /// The native MAF Harness agent.
    /// </summary>
    public AIAgent Agent { get; }

    /// <summary>
    /// The session used by the agent.
    /// </summary>
    public AgentSession Session { get; }

    /// <summary>
    /// Native MAF mode provider, when enabled.
    /// </summary>
    public AgentModeProvider? ModeProvider => Agent.GetService<AgentModeProvider>();

    /// <summary>
    /// Native MAF todo provider, when enabled.
    /// </summary>
    public TodoProvider? TodoProvider => Agent.GetService<TodoProvider>();

    /// <summary>
    /// Runs one Harness request.
    /// </summary>
    public Task<AgentResponse> RunAsync(
        IEnumerable<MafChatMessage> messages,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Agent.RunAsync(messages, Session, options, cancellationToken);
    }

    /// <summary>
    /// Streams one Harness request through the native MAF streaming transport,
    /// including tool calls and approval content.
    /// </summary>
    public IAsyncEnumerable<AgentResponseUpdate> RunStreaming(
        IEnumerable<MafChatMessage> messages,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Agent.RunStreamingAsync(messages, Session, options, cancellationToken);
    }

    /// <summary>
    /// Serializes the MAF session so a host can persist it for resume or fork operations.
    /// </summary>
    public ValueTask<JsonElement> SerializeSessionAsync(
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        return Agent.SerializeSessionAsync(Session, serializerOptions, cancellationToken);
    }

}

#pragma warning restore MAAI001

/// <summary>
/// Extensions for creating the native Microsoft Agent Framework Harness from an AgentKernel run.
/// </summary>
public static class AgentKernelHarnessExtensions
{
    /// <summary>
    /// Builds a native MAF HarnessAgent from an already configured AgentKernel run.
    /// </summary>
    public static async Task<AgentKernelHarness> BuildHarnessAgentAsync(
        this IWantToRun iWantToRun,
        AgentKernelHarnessOptions? options = null,
        AgentSession? agentSession = null,
        JsonElement? serializedSession = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(iWantToRun);

        var kernel = iWantToRun.Kernel
            ?? throw new InvalidOperationException("IWantToRun has not built a chat kernel.");

        options ??= new AgentKernelHarnessOptions();
        ValidateOptions(options);

        var chatClient = AsIChatClient(kernel);
        var harnessOptions = CopyHarnessOptions(
            options.HarnessOptions ?? AgentKernelHarnessOptions.CreateLeastPrivilegeOptions());
        harnessOptions.MaxContextWindowTokens ??= options.MaxContextWindowTokens;
        harnessOptions.MaxOutputTokens ??= options.MaxOutputTokens;
        harnessOptions.UseCompactionSafeTodoList();
        ChatOptionsSanitizer.SanitizeForModel(
            harnessOptions.ChatOptions,
            kernel.ModelName?.Chat);
        var agent = chatClient.AsHarnessAgent(harnessOptions);

        var session = agentSession
            ?? (serializedSession.HasValue
                ? await agent.DeserializeSessionAsync(serializedSession.Value, null, cancellationToken).ConfigureAwait(false)
                : await agent.CreateSessionAsync(cancellationToken).ConfigureAwait(false));
        return new AgentKernelHarness(
            agent,
            session);
    }

    /// <summary>
    /// Converts the provider client selected by AgentKernel to the MAF IChatClient contract.
    /// </summary>
    public static IChatClient AsHarnessChatClient(AiKernel kernel)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        return AsIChatClient(kernel);
    }

    private static IChatClient AsIChatClient(AiKernel kernel)
    {
        return kernel.ChatClient switch
        {
            IChatClient client => client,
            ChatClient client => client.AsIChatClient(),
            _ => throw new NotSupportedException(
                $"The configured AgentKernel ChatClient type '{kernel.ChatClient?.GetType().FullName ?? "null"}' cannot be adapted to MAF IChatClient.")
        };
    }

    private static HarnessAgentOptions CopyHarnessOptions(HarnessAgentOptions source)
    {
        return new HarnessAgentOptions
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            MaxContextWindowTokens = source.MaxContextWindowTokens,
            MaxOutputTokens = source.MaxOutputTokens,
            CompactionStrategy = source.CompactionStrategy,
            DisableCompaction = source.DisableCompaction,
            ChatOptions = source.ChatOptions,
            HarnessInstructions = source.HarnessInstructions,
            ChatHistoryProvider = source.ChatHistoryProvider,
            AIContextProviders = source.AIContextProviders,
            LoopEvaluators = source.LoopEvaluators,
            LoopAgentOptions = source.LoopAgentOptions,
            MaximumIterationsPerRequest = source.MaximumIterationsPerRequest,
            DisableToolAutoApproval = source.DisableToolAutoApproval,
            ToolApprovalAgentOptions = source.ToolApprovalAgentOptions,
            DisableApprovalNotRequiredFunctionBypassing = source.DisableApprovalNotRequiredFunctionBypassing,
            DisableApprovalResponseBinding = source.DisableApprovalResponseBinding,
            DisableFileMemory = source.DisableFileMemory,
            FileMemoryStore = source.FileMemoryStore,
            FileAccessStore = source.FileAccessStore,
            FileAccessProviderOptions = source.FileAccessProviderOptions,
            DisableWebSearch = source.DisableWebSearch,
            DisableTodoProvider = source.DisableTodoProvider,
            DisableAgentModeProvider = source.DisableAgentModeProvider,
            AgentModeProviderOptions = source.AgentModeProviderOptions,
            DisableAgentSkillsProvider = source.DisableAgentSkillsProvider,
            AgentSkillsSource = source.AgentSkillsSource,
            DisableOpenTelemetry = source.DisableOpenTelemetry,
            OpenTelemetrySourceName = source.OpenTelemetrySourceName,
            BackgroundAgents = source.BackgroundAgents,
            BackgroundAgentsProviderOptions = source.BackgroundAgentsProviderOptions
        };
    }

    /// <summary>
    /// Keeps a new user turn visible when MAF compaction is enabled together with the default todo list.
    /// </summary>
    /// <remarks>
    /// MAF 1.28 <c>CompactionMessageIndex.Update</c> treats an unchanged final message as "nothing new"
    /// and leaves the previous index in place. The default todo provider appends the same
    /// "### Current todo list / none yet" user message on every turn, so the real user text is dropped.
    /// A unique revision on that status message makes the final message change without hiding the list.
    /// </remarks>
    public static void UseCompactionSafeTodoList(this HarnessAgentOptions harnessOptions)
    {
        ArgumentNullException.ThrowIfNull(harnessOptions);
        if (!IsCompactionEnabled(harnessOptions) || harnessOptions.DisableTodoProvider)
        {
            return;
        }

        if (harnessOptions.AIContextProviders?.OfType<TodoProvider>().Any() == true)
        {
            return;
        }

        harnessOptions.DisableTodoProvider = true;
        var providers = harnessOptions.AIContextProviders?.ToList() ?? [];
#pragma warning disable MAAI001
        providers.Add(new TodoProvider(new TodoProviderOptions
        {
            TodoListMessageBuilder = FormatCompactionSafeTodoList
        }));
#pragma warning restore MAAI001
        harnessOptions.AIContextProviders = providers;
    }

    private static bool IsCompactionEnabled(HarnessAgentOptions harnessOptions)
    {
        if (harnessOptions.DisableCompaction)
        {
            return false;
        }

        return harnessOptions.CompactionStrategy is not null
            || (harnessOptions.MaxContextWindowTokens is not null && harnessOptions.MaxOutputTokens is not null);
    }

    private static string FormatCompactionSafeTodoList(IReadOnlyList<TodoItem> items)
    {
        var builder = new StringBuilder("### Current todo list");
        if (items.Count == 0)
        {
            builder.Append("\n- none yet");
        }
        else
        {
            foreach (var item in items)
            {
                builder.Append("\n- ")
                    .Append(item.Id)
                    .Append(item.IsComplete ? " [done] " : " [open] ")
                    .Append(item.Title);
                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    builder.Append(": ").Append(item.Description);
                }
            }
        }

        builder.Append("\ncontext-revision: ").Append(Guid.NewGuid().ToString("N"));
        return builder.ToString();
    }

    private static void ValidateOptions(AgentKernelHarnessOptions options)
    {
        if (options.MaxContextWindowTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxContextWindowTokens));
        }

        if (options.MaxOutputTokens < 0 || options.MaxOutputTokens >= options.MaxContextWindowTokens)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxOutputTokens));
        }
    }
}
