using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace Senparc.AI.AgentKernel.Providers.HuggingFace;

public static class HuggingFaceProviderClientFactory
{
    public const string DefaultEndpoint = "https://router.huggingface.co/v1";

    public static IChatClient CreateChatClient(string modelName, string? endpoint = null, string? apiKey = null)
    {
        var openAiClient = CreateOpenAIClient(endpoint, apiKey);
        return openAiClient.GetChatClient(modelName).AsIChatClient();
    }

    public static IEmbeddingGenerator CreateEmbeddingGenerator(string modelName, string? endpoint = null, string? apiKey = null, int? dimensions = null)
    {
        var openAiClient = CreateOpenAIClient(endpoint, apiKey);
        return openAiClient.GetEmbeddingClient(modelName).AsIEmbeddingGenerator(dimensions);
    }

    private static OpenAIClient CreateOpenAIClient(string? endpoint, string? apiKey)
    {
        endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint;

        return new OpenAIClient(
            credential: new ApiKeyCredential(apiKey ?? string.Empty),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint)
            });
    }
}
