using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TextGeneration;
using Sdcb.SparkDesk;

namespace Senparc.AI.Kernel.SparkAI
{
    public class SparkDeskClient  
    {

        private readonly string _appId;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        private readonly static JsonSerializerOptions _defaultJsonEncodingOptions = new() {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkDeskClient"/> class with specified parameters.
        /// </summary>
        /// <param name="appId">The app ID.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="apiSecret">The API Secret.</param>
        public SparkDeskClient(string appId, string apiKey, string apiSecret)
        {
            _appId = appId ?? throw new ArgumentNullException(nameof(appId));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
        }

 
 
         

        /// <summary>
        /// Generates authorization URL for SparkDesk API.
        /// </summary>
        /// <param name="apiKey">SparkDesk API key.</param>
        /// <param name="apiSecret">SparkDesk API secret.</param>
        /// <param name="hostUrl">Host URL. Optional, default is value from const field.</param>
        /// <returns>Authorization URL.</returns>
        public static string GetAuthorizationUrl(string apiKey, string apiSecret, string hostUrl)
        {
            var url = new Uri(hostUrl);

            string dateString = DateTime.UtcNow.ToString("r");

            byte[] signatureBytes = Encoding.ASCII.GetBytes($"host: {url.Host}\ndate: {dateString}\nGET {url.AbsolutePath} HTTP/1.1");

            using HMACSHA256 hmacsha256 = new(Encoding.ASCII.GetBytes(apiSecret));
            byte[] computedHash = hmacsha256.ComputeHash(signatureBytes);
            string signature = Convert.ToBase64String(computedHash);

            string authorizationString = $"api_key=\"{apiKey}\",algorithm=\"hmac-sha256\",headers=\"host date request-line\",signature=\"{signature}\"";
            string authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(authorizationString));

            string query = $"authorization={authorization}&date={dateString}&host={url.Host}";

            return new UriBuilder(url) { Scheme = url.Scheme, Query = query }.ToString();
        }
    }
}
