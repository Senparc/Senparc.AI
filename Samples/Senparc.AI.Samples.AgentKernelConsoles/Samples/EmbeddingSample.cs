using Microsoft.Extensions.VectorData;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Embedding generation and vector search. See KernelConfigExtensions.EmbeddingTests.EmbeddingStoreTest.
/// </summary>
public class EmbeddingSample
{
    private readonly IAiHandler _aiHandler;
    private const string UserId = "Jeffrey";
    private const string CollectionName = "AgentKernelEmbeddingSample";

    public EmbeddingSample(IAiHandler aiHandler)
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
        Console.WriteLine("EmbeddingSample:Generate vectors, write them to the in-memory vector store, then run similarity search.");
        Console.WriteLine("Input stage: enter one text segment per line. Enter n to start the search stage.");
        Console.WriteLine();

        var iWantToRun = agentHandler.IWantTo(setting)
            .ConfigTextEmbeddingModel(UserId, CollectionName)
            .BuildKernel();

        //var vectorStore = iWantToRun.GetVectorStore(setting.VectorDB);
        var store = iWantToRun.CreateTextSearchStore();

        var id = 1ul;
        while (true)
        {
            Console.WriteLine($"[{id}] Enter text(n finish input):");
            var text = Console.ReadLine();
            if (text == "n")
            {
                break;
            }

            if (text.IsNullOrEmpty())
            {
                continue;
            }

            await store.UpsertDocumentsAsync([
                new TextSearchDocument
                {
                    SourceId = id,
                    SourceName = $"doc-{id}",
                    SourceLink = $"local://doc-{id}",
                    Text = text
                }
            ]);

            var vec = await iWantToRun.GetEmbeddingAsync(text);
            Console.WriteLine($"[Debug] Written doc-{id}, vector dimensions:{vec.Length}");
            id++;
        }

        Console.WriteLine();
        Console.WriteLine("Retrieval phase: enter a question, or enter exit to leave.");

        while (true)
        {
            Console.WriteLine("Enter a question:");
            var question = Console.ReadLine();
            if (question.IsNullOrEmpty())
            {
                continue;
            }

            if (question.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var dt = DateTime.Now;
            var hits = await store.SearchAsync(question, 3);
            var j = 0;
            foreach (var item in hits)
            {
                j++;
                Console.WriteLine($"Response result[{j}]:");
                Console.WriteLine($"  Id: {item.SourceId}");
                Console.WriteLine($"  Name: {item.SourceName}");
                Console.WriteLine($"  Text: {item.Text}");
                Console.WriteLine();
            }

            if (j == 0)
            {
                Console.WriteLine("No matching results");
            }

            Console.WriteLine($"[Debug] Search elapsed time {(DateTime.Now - dt).TotalMilliseconds}ms");
            Console.WriteLine();
        }
    }
}
