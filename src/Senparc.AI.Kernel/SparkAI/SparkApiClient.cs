
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using Microsoft.SemanticKernel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text.Json;
using Microsoft.SemanticKernel.TextGeneration;
namespace Senparc.AI.Kernel
{
    public class SparkApiClient : ITextGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _modelVersion;
        public class StreamingTextContent
        {
            public string Text { get; }
            public Encoding Encoding { get; }

            public StreamingTextContent(string text, Encoding encoding)
            {
                Text = text;
                Encoding = encoding;
            }
        }
        public class ChatResponse
        {
            public string Text { get; set; }
            // Add other properties as needed to match the API response
        }
        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        public SparkApiClient(string apiKey, string modelVersion, string baseUrl = "https://spark-api-open.xf-yun.com/v1/chat/completions")
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _modelVersion = modelVersion ?? throw new ArgumentNullException(nameof(modelVersion));
        }


        //public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Microsoft.SemanticKernel.Kernel? kernel = null, CancellationToken cancellationToken = default)
        //{
        //    var messages = new List<object>
        //    {
        //    new { role = "user", content = prompt }
        //};
        //    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        //    var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(new {
        //        model = _modelVersion,
        //        messages,
        //        stream = true
        //    }), Encoding.UTF8, "application/json");

        //    var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl) {
        //        Content = requestContent
        //    };

        //    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        //    response.EnsureSuccessStatusCode();

        //    using var responseStream = await response.Content.ReadAsStreamAsync();
        //    using var streamReader = new StreamReader(responseStream, Encoding.UTF8);

        //    string line;
        //    while ((line = await streamReader.ReadLineAsync()) != null) {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        yield return new StreamingTextContent(line, Encoding.UTF8);
        //    }
        //}


        public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Microsoft.SemanticKernel.Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var messages = new List<object>
            {
            new { role = "user", content = prompt }
        };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var requestBody = new {
                model = _modelVersion,
                messages,
                stream = false // Ensure streaming is disabled for full response
            };

            var jsonString = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(_baseUrl, requestContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Check for cancellation before reading the response
            cancellationToken.ThrowIfCancellationRequested();

            var responseJson = await response.Content.ReadAsStringAsync();

            // Check for cancellation after reading the response
            cancellationToken.ThrowIfCancellationRequested();

            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseJson);

            return new List<TextContent>
            {
            new TextContent { Text = chatResponse.Text }
        };
        }

        public async IAsyncEnumerable<Microsoft.SemanticKernel.StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings, Microsoft.SemanticKernel.Kernel? kernel, CancellationToken cancellationToken)
        {
            var messages = new List<object>
          {
            new { role = "user", content = prompt }
        };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(new {
                model = _modelVersion,
                messages,
                stream = true
            }), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl) {
                Content = requestContent
            };

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(responseStream, Encoding.UTF8);

            string line;
            while ((line = await streamReader.ReadLineAsync()) != null) {
                cancellationToken.ThrowIfCancellationRequested();

                yield return new Microsoft.SemanticKernel.StreamingTextContent(line);
            }
        }



        //public async Task<string> GetChatCompletionAsync(string model, string userMessage)
        //    {
        //        if (string.IsNullOrWhiteSpace(model))
        //            throw new ArgumentException("Model cannot be null or empty.", nameof(model));
        //        if (string.IsNullOrWhiteSpace(userMessage))
        //            throw new ArgumentException("User message cannot be null or empty.", nameof(userMessage));

        //        var requestUrl = $"{_baseUrl}/chat/completions";
        //        var requestBody = new {
        //            model,
        //            messages = new[]
        //            {
        //            new { role = "user", content = userMessage }
        //        }
        //        };
        //        var requestJson = JsonConvert.SerializeObject(requestBody);
        //        using var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
        //        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        //        try {
        //            using var response = await _httpClient.PostAsync(requestUrl, requestContent);
        //            response.EnsureSuccessStatusCode();

        //            var responseJson = await response.Content.ReadAsStringAsync();
        //            dynamic responseObject = JsonConvert.DeserializeObject(responseJson);

        //            return responseObject.choices[0].message.content;
        //        } catch (HttpRequestException e) {
        //            throw new Exception("An error occurred while sending the request.", e);
        //        } catch (JsonException e) {
        //            throw new Exception("An error occurred while parsing the response.", e);
        //        }
        //    }
    }
}
