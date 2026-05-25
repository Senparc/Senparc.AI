using Microsoft.Extensions.VectorData;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.KernelConfigExtensions;

namespace Senparc.AI.AgentKernel;

/// <summary>
/// RAG 向量检索辅助类，逻辑参考 AgentKernel.Tests KernelConfigExtensions.EmbeddingTests。
/// </summary>
public class TextSearchDocument
{
    public ulong SourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string SourceLink { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class TextSearchRecord
{
    public ulong SourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string SourceLink { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public ReadOnlyMemory<float> Embedding { get; set; }
}

public class TextSearchStore
{
    private readonly VectorStoreCollection<ulong, TextSearchRecord> _collection;

    public IWantToRun IWantToRun { get; }

    public TextSearchStore(IWantToRun iWantToRun, VectorStore vectorStore)
    {
        IWantToRun = iWantToRun;
        var kernel = iWantToRun.Kernel;

        var definition = new VectorStoreCollectionDefinition
        {
            Properties =
            [
                new VectorStoreKeyProperty("SourceId", typeof(ulong)),
                new VectorStoreDataProperty("SourceName", typeof(string)),
                new VectorStoreDataProperty("SourceLink", typeof(string)),
                new VectorStoreDataProperty("Text", typeof(string)),
                new VectorStoreVectorProperty("Embedding", typeof(ReadOnlyMemory<float>), kernel.EmbeddingDimensions),
            ],
            EmbeddingGenerator = kernel.EmbeddingGenerator
        };
        _collection = vectorStore.GetCollection<ulong, TextSearchRecord>(kernel.EmbeddingCollectionName, definition);
    }

    public async Task UpsertDocumentsAsync(IEnumerable<TextSearchDocument> documents)
    {
        await _collection.EnsureCollectionExistsAsync();
        foreach (var doc in documents)
        {
            var record = new TextSearchRecord
            {
                SourceId = doc.SourceId,
                SourceName = doc.SourceName,
                SourceLink = doc.SourceLink,
                Text = doc.Text,
                Embedding = await IWantToRun.GetEmbeddingAsync(doc.Text)
            };
            await _collection.UpsertAsync(record);
        }
    }

    public async Task<IEnumerable<TextSearchDocument>> SearchAsync(string query, int topK, CancellationToken cancellationToken = default)
    {
        var results = _collection.SearchAsync(query, topK, cancellationToken: cancellationToken);
        var documents = new List<TextSearchDocument>();
        await foreach (var result in results)
        {
            documents.Add(new TextSearchDocument
            {
                SourceId = result.Record.SourceId,
                SourceName = result.Record.SourceName,
                SourceLink = result.Record.SourceLink,
                Text = result.Record.Text
            });
        }
        return documents;
    }

    public static IEnumerable<TextSearchDocument> GetSampleDocuments()
    {
        yield return new TextSearchDocument
        {
            SourceId = 1,
            SourceName = "NeuCharFramework (NCF) - Overview",
            SourceLink = "https://doc.ncf.pub/",
            Text = "NeuCharFramework (NCF) is an AI-enabled, out-of-the-box modular development framework. It provides quick setup with templates and one-click installation, integrates basic AI capabilities for easier AI application development, and emphasizes modular design and extensibility."
        };
        yield return new TextSearchDocument
        {
            SourceId = 2,
            SourceName = "Get Started - NCF",
            SourceLink = "https://doc.ncf.pub/start/start-develop/get-ncf-template.html",
            Text = "The Get Started guide provides framework templates and a one-click installation experience to quickly bootstrap projects based on NCF, with step-by-step instructions for obtaining and using the project templates."
        };
        yield return new TextSearchDocument
        {
            SourceId = 3,
            SourceName = "About NCF - Project Introduction",
            SourceLink = "https://doc.ncf.pub/start/instruction/about-ncf.html",
            Text = "NCF highlights: modular design, multi-database support (SQL Server, MySQL, SQLite, PostgreSQL, Oracle, DM), high performance, DDD patterns, Dapr microservice support, backward compatibility for different deployment modes, and maintained community support by the Senparc developer community."
        };
    }
}
