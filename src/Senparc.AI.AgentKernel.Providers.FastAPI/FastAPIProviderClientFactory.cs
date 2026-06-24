using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace Senparc.AI.AgentKernel.Providers.FastAPI;

public static class FastAPIProviderClientFactory
{
    public static IChatClient CreateChatClient(string modelName, string endpoint, string? apiKey = null)
    {
        var openAiClient = CreateOpenAIClient(endpoint, apiKey);
        return openAiClient.GetChatClient(modelName).AsIChatClient();
    }

    public static IEmbeddingGenerator CreateEmbeddingGenerator(string modelName, string endpoint, string? apiKey = null, int? dimensions = null)
    {
        var openAiClient = CreateOpenAIClient(endpoint, apiKey);
        return openAiClient.GetEmbeddingClient(modelName).AsIEmbeddingGenerator(dimensions);
    }

    private static OpenAIClient CreateOpenAIClient(string endpoint, string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("FastAPI provider requires a non-empty endpoint.", nameof(endpoint));
        }

        return new OpenAIClient(
            credential: new ApiKeyCredential(apiKey ?? string.Empty),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint)
            });
    }
}
