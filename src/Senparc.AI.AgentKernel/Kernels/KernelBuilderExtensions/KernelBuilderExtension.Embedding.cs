using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using OpenAI.Embeddings;
using Senparc.AI.AgentKernel.Providers.FastAPI;
using Senparc.AI.AgentKernel.Providers.HuggingFace;
using System.ClientModel;

namespace Senparc.AI.AgentKernel.Kernels.KernelBuilderExtensions
{
    public static partial class KernelBuilderExtension
    {
        #region AddEmbedding

        public static EmbeddingClient AddOpenAIEmbedding(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName)
        {
            return new EmbeddingClient(modelName, apiKey);
        }

        public static EmbeddingClient AddOpenAICompatibleEmbedding(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName,
            string endpoint)
        {
            var client = new OpenAIClient(
                credential: new ApiKeyCredential(apiKey ?? string.Empty),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint)
                });

            return client.GetEmbeddingClient(modelName);
        }

        public static EmbeddingClient AddAzureOpenAIEmbedding(this IAIKernelBuilder kernelBuilder,
            Uri endpoint,
            ApiKeyCredential credential,
            AzureOpenAIClientOptions options,
            string azureDeploymentName)
        {
            return new AzureOpenAIClient(endpoint, credential, options).GetEmbeddingClient(azureDeploymentName);
        }

        public static EmbeddingClient AddNeuCharAIEmbedding(this IAIKernelBuilder kernelBuilder,
            Uri endpoint,
            ApiKeyCredential credential,
            AzureOpenAIClientOptions options,
            string modelName)
        {
            return AddAzureOpenAIEmbedding(kernelBuilder, endpoint, credential, options, modelName);
        }

        public static OllamaApiClient AddOllamaEmbedding(this IAIKernelBuilder kernelBuilder,
            string endpoint,
            string modelName)
        {
            return new OllamaApiClient(endpoint, modelName);
        }

        public static IEmbeddingGenerator AddFastAPIEmbedding(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName,
            string endpoint,
            int? dimensions = null)
        {
            return FastAPIProviderClientFactory.CreateEmbeddingGenerator(
                modelName: modelName,
                endpoint: endpoint,
                apiKey: apiKey,
                dimensions: dimensions);
        }

        public static IEmbeddingGenerator AddHuggingFaceEmbedding(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName,
            string? endpoint,
            int? dimensions = null)
        {
            return HuggingFaceProviderClientFactory.CreateEmbeddingGenerator(
                modelName: modelName,
                endpoint: endpoint,
                apiKey: apiKey,
                dimensions: dimensions);
        }

        public static EmbeddingClient AddDeepSeekEmbedding(this IAIKernelBuilder kernelBuilder,
            string apiKey,
            string modelName,
            string endpoint)
        {
            return kernelBuilder.AddOpenAICompatibleEmbedding(apiKey, modelName, endpoint);
        }

        #endregion
    }
}
