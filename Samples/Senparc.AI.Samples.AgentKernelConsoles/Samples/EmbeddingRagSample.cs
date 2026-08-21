using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// RAG sample. See KernelConfigExtensions.EmbeddingTests.EmbeddingTest.
/// </summary>
public class EmbeddingRagSample
{
    private readonly IAiHandler _aiHandler;
    private const string UserId = "Jeffrey";
    private const string CollectionName = "AgentKernelRagSample";

    private TextSearchStore? _textSearchStore;

    public EmbeddingRagSample(IAiHandler aiHandler)
    {
        _aiHandler = aiHandler;
        if (aiHandler is AgentAiHandler h)
        {
            h.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);
        }
    }

    public async Task RunAsync()
    {
        if (_aiHandler is not AgentAiHandler agentHandler)
        {
            throw new InvalidOperationException("This sample requires AgentAiHandler.");
        }

        var setting = SampleSetting.CurrentSetting;
        Console.WriteLine("EmbeddingRagSample:Use TextSearchProvider for retrieval-augmented generation.");
        Console.WriteLine("[Debug] Initializing the vector store and writing NCF sample documents...");

        var textSearchOptions = new TextSearchProviderOptions
        {
            SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
            CitationsPrompt = "Always cite sources at the end of your response using the format: **Source:** [SourceName](SourceLink)",
            RecentMessageMemoryLimit = 6,
        };

        async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(string text, CancellationToken ct)
        {
            var searchResults = await _textSearchStore!.SearchAsync(text, 5, ct);
            return searchResults.Select(r => new TextSearchProvider.TextSearchResult
            {
                SourceName = r.SourceName,
                SourceLink = r.SourceLink,
                Text = r.Text,
                RawRepresentation = r
            });
        }

        var chatOptions = new ChatClientAgentOptions
        {
            ChatOptions = new()
            {
                Instructions = "You are a helpful support specialist. Answer questions using the provided context and cite the source document when available."
            },
            AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
        };

        var iWantToRun = await agentHandler.IWantTo(setting)
            .ConfigModel(ConfigModel.Chat, UserId)
            .ConfigTextEmbeddingModel(UserId, CollectionName)
            .BuildKernelWithAgentSessionAsync(chatOptions);

        var vectorStore = iWantToRun.CreateTextSearchStore();
        await vectorStore.UpsertDocumentsAsync(TextSearchStore.GetSampleDocuments());

        Console.WriteLine("[Debug] Vector store initialization completed. You can ask, for example: What is NCF?");
        Console.WriteLine("Enter exit to leave.");
        Console.WriteLine();

        AgentSession? session = iWantToRun.Kernel.AgentSession;

        while (true)
        {
            Console.WriteLine("Human:");
            var input = Console.ReadLine();
            if (input.IsNullOrEmpty())
            {
                continue;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine("Assistant:");
            try
            {
                var result = await iWantToRun.RunChatAsync(input, session);
                Console.WriteLine(result.Result.Text);
                Console.WriteLine($"[Debug] Tokens — total: {result.Result.Usage?.TotalTokenCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:" + ex);
            }

            Console.WriteLine();
        }
    }
}
