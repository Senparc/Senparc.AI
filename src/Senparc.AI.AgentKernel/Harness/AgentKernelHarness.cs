/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    File: AgentKernelHarness.cs
    Description: Native Microsoft Agent Framework Harness integration for AgentKernel.

    Created by: Senparc - 20260911

    Modified by: Senparc - 20260916
    Description: v0.1.14-preview3 unified Harness model parameter and transport compatibility

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
using System.Runtime.CompilerServices;
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
            DisableFileAccess = true,
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
    internal AgentKernelHarness(AIAgent agent, AgentSession session, bool supportsStreaming)
    {
        Agent = agent;
        Session = session;
        SupportsStreaming = supportsStreaming;
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
    /// Whether the selected provider supports stable streaming responses.
    /// </summary>
    public bool SupportsStreaming { get; }

    /// <summary>
    /// Determines whether a provider can use the streaming Harness transport.
    /// NeuCharAI currently uses the non-streaming transport and is converted to
    /// response updates by <see cref="RunStreaming"/>.
    /// </summary>
    public static bool SupportsStreamingFor(ISenparcAiSetting? setting)
    {
        return setting?.AiPlatform != AiPlatform.NeuCharAI;
    }

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
    /// Runs one Harness request through the provider-compatible update transport,
    /// including tool calls and approval content. Providers without stable
    /// streaming support return their complete response as response updates.
    /// </summary>
    public IAsyncEnumerable<AgentResponseUpdate> RunStreaming(
        IEnumerable<MafChatMessage> messages,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return RunStreamingCompatibleAsync(messages, options, cancellationToken);
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

    private async IAsyncEnumerable<AgentResponseUpdate> RunStreamingCompatibleAsync(
        IEnumerable<MafChatMessage> messages,
        AgentRunOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (SupportsStreaming)
        {
            await foreach (var update in Agent.RunStreamingAsync(
                               messages,
                               Session,
                               options,
                               cancellationToken)
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
            {
                yield return update;
            }

            yield break;
        }

        var response = await RunAsync(messages, options, cancellationToken).ConfigureAwait(false);
        foreach (var update in response.ToAgentResponseUpdates())
        {
            yield return update;
        }
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
        var harnessOptions = options.HarnessOptions ?? AgentKernelHarnessOptions.CreateLeastPrivilegeOptions();
        ChatOptionsSanitizer.SanitizeForModel(
            harnessOptions.ChatOptions,
            kernel.ModelName?.Chat);
        var agent = chatClient.AsHarnessAgent(
            options.MaxContextWindowTokens,
            options.MaxOutputTokens,
            harnessOptions);

        var session = agentSession
            ?? (serializedSession.HasValue
                ? await agent.DeserializeSessionAsync(serializedSession.Value, null, cancellationToken).ConfigureAwait(false)
                : await agent.CreateSessionAsync(cancellationToken).ConfigureAwait(false));
        return new AgentKernelHarness(
            agent,
            session,
            AgentKernelHarness.SupportsStreamingFor(kernel.SenparcAiSetting));
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
