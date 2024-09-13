using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.SemanticKernel;

namespace Senparc.AI.Kernel.Helpers
{
    internal static class ExtensionHelper
    {
        /// <summary>
        /// 设置 KernelArguments
        /// </summary>
        /// <param name="kernelArguments"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(this KernelArguments kernelArguments, string key, object value)
        {
            kernelArguments[key] = value;
        }

        public static string GenerateApiPassword(string key, string secret)
        {
            // 拼接 key 和 secret
            string combined = $"{key}:{secret}";

            // 将拼接后的字符串进行 Base64 编码
            byte[] byteArray = Encoding.UTF8.GetBytes(combined);
            string base64Encoded = Convert.ToBase64String(byteArray);

            return base64Encoded;
        }

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
