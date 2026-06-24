using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Providers.FastAPI;
using Senparc.AI.AgentKernel.Providers.HuggingFace;

namespace Senparc.AI.AgentKernel.Tests.Helpers
{
    [TestClass]
    public class ProviderFactoriesTests
    {
        [TestMethod]
        public void HuggingFaceFactory_ShouldCreateClients_WithDefaultEndpoint()
        {
            var chatClient = HuggingFaceProviderClientFactory.CreateChatClient(
                modelName: "test-model",
                endpoint: null,
                apiKey: "hf-key");

            var embeddingGenerator = HuggingFaceProviderClientFactory.CreateEmbeddingGenerator(
                modelName: "test-embedding-model",
                endpoint: null,
                apiKey: "hf-key",
                dimensions: 1024);

            Assert.IsNotNull(chatClient);
            Assert.IsNotNull(embeddingGenerator);
            Assert.IsInstanceOfType(chatClient, typeof(IChatClient));
            Assert.IsInstanceOfType(embeddingGenerator, typeof(IEmbeddingGenerator));
        }

        [TestMethod]
        public void FastAPIFactory_ShouldCreateClients_WithEndpoint()
        {
            var chatClient = FastAPIProviderClientFactory.CreateChatClient(
                modelName: "test-model",
                endpoint: "https://fastapi.example.com/v1",
                apiKey: "fast-key");

            var embeddingGenerator = FastAPIProviderClientFactory.CreateEmbeddingGenerator(
                modelName: "test-embedding-model",
                endpoint: "https://fastapi.example.com/v1",
                apiKey: "fast-key",
                dimensions: 1024);

            Assert.IsNotNull(chatClient);
            Assert.IsNotNull(embeddingGenerator);
            Assert.IsInstanceOfType(chatClient, typeof(IChatClient));
            Assert.IsInstanceOfType(embeddingGenerator, typeof(IEmbeddingGenerator));
        }

        [TestMethod]
        public void FastAPIFactory_ShouldThrow_WhenEndpointMissing()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                FastAPIProviderClientFactory.CreateChatClient(
                    modelName: "test-model",
                    endpoint: string.Empty,
                    apiKey: "fast-key"));
        }
    }
}
