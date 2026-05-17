using Microsoft.Agents.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenAI;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Senparc.AI.AgentKernel.Handlers;

namespace Senparc.AI.AgentKernel.Tests.KernelConfigExtensions
{

    //ref: https://jamiemaguire.net/index.php/2026/02/21/microsoft-agent-framework-adding-rag-to-your-ai-agent-using-textsearchprovider-and-in-memory-vector-store/

    public class TextSearchDocument
    {
        public string SourceId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string SourceLink { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class TextSearchRecord
    {
        public string SourceId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string SourceLink { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Embedding { get; set; } = string.Empty;
    }


    public class TextSearchStore
    {
        private readonly VectorStoreCollection<string, TextSearchRecord> _collection;
        public TextSearchStore(InMemoryVectorStore vectorStore, string collectionName, int dimensions)
        {
            var definition = new VectorStoreCollectionDefinition
            {
                Properties =
                [
                    new VectorStoreKeyProperty("SourceId", typeof(string)),
                new VectorStoreDataProperty("SourceName", typeof(string)),
                new VectorStoreDataProperty("SourceLink", typeof(string)),
                new VectorStoreDataProperty("Text", typeof(string)),
                new VectorStoreVectorProperty("Embedding", typeof(string), dimensions),
            ]
            };
            _collection = vectorStore.GetCollection<string, TextSearchRecord>(collectionName, definition);
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
                    Embedding = doc.Text
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
                SourceId = "beginner-strength-001",
                SourceName = "Iron Mind AI - Beginner Strength Training Guide",
                SourceLink = "https://ironmindai.com/tips/beginner-strength",
                Text = "For beginners, focus on compound movements like squats, deadlifts, bench press, " +
                       "and overhead press. Train 3-4 days per week with at least one rest day between " +
                       "sessions. Start with a weight you can control for 8-12 reps with good form. " +
                       "Progressive overload is key - aim to gradually increase weight, reps, or sets " +
                       "over time. Consistency beats intensity in the early stages."
            };
            yield return new TextSearchDocument
            {
                SourceId = "nutrition-basics-001",
                SourceName = "Iron Mind AI - Nutrition for Muscle Growth",
                SourceLink = "https://ironmindai.com/tips/nutrition-muscle-growth",
                Text = "To support muscle growth, aim for 1.6 to 2.2 grams of protein per kilogram of " +
                       "body weight per day. Spread protein intake across 3-5 meals for optimal muscle " +
                       "protein synthesis. Prioritize whole food sources like chicken, fish, eggs, Greek " +
                       "yogurt, and legumes. Don't neglect carbohydrates - they fuel your workouts and " +
                       "aid recovery. A slight caloric surplus of 200-300 calories above maintenance is " +
                       "ideal for lean muscle gain."
            };
            yield return new TextSearchDocument
            {
                SourceId = "recovery-sleep-001",
                SourceName = "Iron Mind AI - Recovery and Sleep Guide",
                SourceLink = "https://ironmindai.com/tips/recovery-sleep",
                Text = "Sleep is when your body repairs and builds muscle tissue. Aim for 7-9 hours of " +
                       "quality sleep per night. Poor sleep increases cortisol levels, which can impair " +
                       "muscle recovery and promote fat storage. Establish a consistent sleep schedule, " +
                       "limit screen time before bed, and keep your room cool and dark. Active recovery " +
                       "on rest days - such as walking, stretching, or light yoga - also helps reduce " +
                       "soreness and improve circulation."
            };
        }


        public class IronMindRagAgent
        {
            private const int EmbeddingDimensions = 3072;
            private const string CollectionName = "iron-mind-ai-tips";
            private readonly TextSearchStore _textSearchStore;
            public IronMindRagAgent(OpenAIClient openAIClient, string embeddingModel)
            {
                var vectorStore = new InMemoryVectorStore(new()
                {
                    EmbeddingGenerator = openAIClient.GetEmbeddingClient(embeddingModel).AsIEmbeddingGenerator()
                });
                _textSearchStore = new TextSearchStore(vectorStore, CollectionName, EmbeddingDimensions);
            }
            public async Task<AIAgent> CreateAgentAsync(OpenAIClient openAIClient, string model)
            {
                await _textSearchStore.UpsertDocumentsAsync(TextSearchStore.GetSampleDocuments());
                var textSearchOptions = new TextSearchProviderOptions
                {
                    SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
                    CitationsPrompt = "Always cite sources at the end of your response using the format: " +
                                      "**Source:** [SourceName](SourceLink)",
                };
                return openAIClient
                    .GetChatClient(model)
                    .AsAIAgent(new ChatClientAgentOptions
                    {
                        ChatOptions = new()
                        {
                            Instructions = "You are Iron Mind AI, a knowledgeable personal trainer. " +
                                           "You MUST base your answers on the provided context documents. " +
                                           "Always cite your sources by name and link at the end of your response. " +
                                           "If the context does not contain relevant information, say so."
                        },
                        AIContextProviderFactory = (ctx, ct) => new ValueTask<AIContextProvider>(
                            new TextSearchProvider(SearchAsync, ctx.SerializedState,
                                                  ctx.JsonSerializerOptions, textSearchOptions)),
                        ChatHistoryProviderFactory = (ctx, ct) => new ValueTask<ChatHistoryProvider>(
                            new InMemoryChatHistoryProvider().WithAIContextProviderMessageRemoval()),
                    });
            }
            private async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAsync(
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
        }

        [TestClass]
        public class KernelConfigExtensions : KernelTestBase
        {
            private readonly TextSearchStore _textSearchStore;

            [TestMethod]
            public void InMemoryTest()
            {
                AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

                var iWantToRun =
                agentAiHandler.IWantTo()
                            .ConfigTextEmbeddingModel("Jeffrey","EmbeddingTest",)
                            .BuildKernel();

                var vectorStore = new InMemoryVectorStore(new()
                {
                    EmbeddingGenerator = openAIClient.GetEmbeddingClient(embeddingModel).AsIEmbeddingGenerator()
                });


                _textSearchStore = new TextSearchStore(vectorStore, CollectionName, EmbeddingDimensions);

            }

        }
    }
}