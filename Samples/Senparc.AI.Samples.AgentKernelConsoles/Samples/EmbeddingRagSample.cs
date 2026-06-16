using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// RAG 示例，参考 KernelConfigExtensions.EmbeddingTests.EmbeddingTest。
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
            throw new InvalidOperationException("当前示例需要 AgentAiHandler。");
        }

        var setting = SampleSetting.CurrentSetting;
        Console.WriteLine("EmbeddingRagSample：使用 TextSearchProvider 进行检索增强生成。");
        Console.WriteLine("[调试] 正在初始化向量库并写入 NCF 示例文档…");

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

        Console.WriteLine("[调试] 向量库初始化完成。可提问例如：What is NCF?");
        Console.WriteLine("输入 exit 退出。");
        Console.WriteLine();

        AgentSession? session = iWantToRun.Kernel.AgentSession;

        while (true)
        {
            Console.WriteLine("人类：");
            var input = Console.ReadLine();
            if (input.IsNullOrEmpty())
            {
                continue;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine("机器：");
            try
            {
                var result = await iWantToRun.RunChatAsync(input, session);
                Console.WriteLine(result.Result.Text);
                Console.WriteLine($"[调试] Tokens — total: {result.Result.Usage?.TotalTokenCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生错误：" + ex);
            }

            Console.WriteLine();
        }
    }
}
