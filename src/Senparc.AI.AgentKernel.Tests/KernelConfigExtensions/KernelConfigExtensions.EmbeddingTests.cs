using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenAI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.AgentKernel.KernelConfigExtensions;

namespace Senparc.AI.AgentKernel.Tests.KernelConfigExtensions
{

    // SemanticKernel Connectors solution ref: https://jamiemaguire.net/index.php/2026/02/21/microsoft-agent-framework-adding-rag-to-your-ai-agent-using-textsearchprovider-and-in-memory-vector-store/

    public class TextSearchDocument
    {
        //public string SourceId { get; set; } = string.Empty;
        public ulong SourceId { get; set; } = 0;
        public string SourceName { get; set; } = string.Empty;
        public string SourceLink { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class TextSearchRecord
    {
        // public string SourceId { get; set; } = string.Empty;
        //[VectorStoreKey]
        public ulong SourceId { get; set; } = 0;
        //[VectorStoreData(IsIndexed = true)]
        public string SourceName { get; set; } = string.Empty;
        //[VectorStoreData(IsFullTextIndexed = true)]
        public string SourceLink { get; set; } = string.Empty;
        //[VectorStoreData(IsFullTextIndexed = true)]
        public string Text { get; set; } = string.Empty;
        //[VectorStoreVector(dimensions: 3072 /*根据模型调整，例如 text-embedding-ada-002 为 1536，Large 为 3072*/, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
        public ReadOnlyMemory<float> Embedding { get; set; } = null;
    }


    public class TextSearchStore
    {
        private readonly VectorStoreCollection<ulong, TextSearchRecord> _collection;

        public IWantToRun IWantToRun { get; }

        public TextSearchStore(IWantToRun iWantToRun, VectorStore vectorStore)
        {
            IWantToRun = iWantToRun;
            var kernel = IWantToRun.Kernel;

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

        public async Task<IEnumerable<TextSearchDocument>> SearchAsync(
            string query, int topK, CancellationToken cancellationToken = default)
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
                SourceId = 1,// "NCF-1",
                SourceName = "NeuCharFramework (NCF) - Overview",
                SourceLink = "https://doc.ncf.pub/",
                Text = "NeuCharFramework (NCF) is an AI-enabled, out-of-the-box modular development framework. It provides quick setup with templates and one-click installation, integrates basic AI capabilities for easier AI application development, and emphasizes modular design and extensibility."
            };
            yield return new TextSearchDocument
            {
                SourceId = 2,// "NCF-2",
                SourceName = "Get Started - NCF",
                SourceLink = "https://doc.ncf.pub/start/start-develop/get-ncf-template.html",
                Text = "The Get Started guide provides framework templates and a one-click installation experience to quickly bootstrap projects based on NCF, with step-by-step instructions for obtaining and using the project templates."
            };
            yield return new TextSearchDocument
            {
                SourceId = 3,//"NCF-3",
                SourceName = "About NCF - Project Introduction",
                SourceLink = "https://doc.ncf.pub/start/instruction/about-ncf.html",
                Text = "NCF highlights: modular design, multi-database support (SQL Server, MySQL, SQLite, PostgreSQL, Oracle, DM), high performance, DDD patterns, Dapr microservice support, backward compatibility for different deployment modes, and maintained community support by the Senparc developer community."
            };
        }

        [TestClass]
        public class KernelConfigExtensions : KernelTestBase
        {
            private TextSearchStore _textSearchStore;


            private async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(
               string text, CancellationToken ct)
            {
                var searchResults = await _textSearchStore.SearchAsync(text, 2, ct);
                return searchResults.Select(r => new TextSearchProvider.TextSearchResult
                {
                    SourceName = r.SourceName,
                    SourceLink = r.SourceLink,
                    Text = r.Text,
                    RawRepresentation = r
                });
            }

            [TestMethod]
            public async Task EmbeddingStoreTest()
            {
                AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);
                var iWantToRun =
                        agentAiHandler.IWantTo()
                        .ConfigTextEmbeddingModel("Jeffrey", "EmbeddingTest")
                        .BuildKernel();

                var result = await iWantToRun.GetEmbeddingAsync("Senparc");

                Assert.AreEqual(_senparcAiSetting.ModelName.EmbeddingDimensions, result.Length);
                Console.WriteLine(result.ToJson(true));
                Console.WriteLine(result.ToArray().ToJson(true));
            }

            [TestMethod]
            public async Task EmbeddingTest()
            {
                // Search Configuration Options
                TextSearchProviderOptions textSearchOptions = new()
                {
                    SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
                    CitationsPrompt = "Always cite sources at the end of your response using the format: " +
                          "**Source:** [SourceName](SourceLink)",
                    RecentMessageMemoryLimit = 6,
                };

                var chatOptions = new ChatClientAgentOptions()
                {
                    ChatOptions = new()
                    {
                        Instructions = "You are a helpful support specialist. Answer questions using the provided context and cite the source document when available."
                    },
                    AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
                };

                // Create IWantToRun instance with the configured options
                AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

                var iWantToRun =
                agentAiHandler.IWantTo()
                            .ConfigModel(ConfigModel.Chat, "Jeffrey")
                            .ConfigTextEmbeddingModel("Jeffrey", "EmbeddingTest")
                            .BuildKernel(chatOptions);

                // Get the AI kernel and vector store from the IWantToRun instance
                var aiKernel = iWantToRun.Kernel;
                VectorStore vectorStore = iWantToRun.GetVectorStore(_senparcAiSetting.VectorDB);

                // Initialize the TextSearchStore with the vector store and embedding configuration
                _textSearchStore = new TextSearchStore(iWantToRun, vectorStore);

                await _textSearchStore.UpsertDocumentsAsync(TextSearchStore.GetSampleDocuments());

                Console.WriteLine("Embedding Store Finished");


                var searchResults = await _textSearchStore.SearchAsync("What is NCF?", 2, default(CancellationToken));
                var result = searchResults.Select(r => new TextSearchProvider.TextSearchResult
                {
                    SourceName = r.SourceName,
                    SourceLink = r.SourceLink,
                    Text = r.Text,
                    RawRepresentation = r
                });
                Console.WriteLine(result.ToJson(true));
                Assert.AreEqual(2, result.Count());

            }

        }
    }
}