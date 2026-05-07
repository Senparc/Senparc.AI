using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;

namespace Senparc.AI.Kernel.Helpers;

internal class MafRuntimeClient
{
    private readonly HttpClient _httpClient;

    public MafRuntimeClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string> ChatAsync(ISenparcAiSetting setting, string modelName, string content, PromptConfigParameter? parameter = null, CancellationToken cancellationToken = default)
    {
        var endpoint = setting.Endpoint?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("AI endpoint 未配置。");
        }

        var requestBody = new
        {
            model = modelName,
            messages = new[]
            {
                new { role = "user", content }
            },
            temperature = parameter?.Temperature,
            top_p = parameter?.TopP,
            max_tokens = parameter?.MaxTokens
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/v1/chat/completions");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        if (!string.IsNullOrWhiteSpace(setting.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", setting.ApiKey);
        }

        if (!string.IsNullOrWhiteSpace(setting.OrganizationId))
        {
            request.Headers.Add("OpenAI-Organization", setting.OrganizationId);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"请求失败：{response.StatusCode}，Body: {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var contentNode = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return contentNode ?? string.Empty;
    }
}
