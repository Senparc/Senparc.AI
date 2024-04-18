using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel
{
    //public class CustomHttpMessageHandler : HttpClientHandler
    //{
    //    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    //    {
    //        var requestBody = await request.Content.ReadAsStringAsync();
    //        Console.WriteLine($"Request method: {request.Method}, Request URI: {request.RequestUri}, Request body: {requestBody}");

    //        return await base.SendAsync(request, cancellationToken);
    //    }
    //}

    public class LoggingHttpMessageHandler : DelegatingHandler
    {
        public LoggingHttpMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Request: {request.Method} {request.RequestUri}");

            if (request.Content != null)
            {
                string requestBody = await request.Content.ReadAsStringAsync();
                Console.WriteLine($"Request Body: {requestBody}");
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine($"Response: {(int)response.StatusCode} {response.StatusCode}");

            if (response.Content != null)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Body: {responseBody}");
            }

            return response;
        }
    }
}
