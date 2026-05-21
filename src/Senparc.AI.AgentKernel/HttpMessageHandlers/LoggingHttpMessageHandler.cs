using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.HttpMessageHandlers
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
        public bool EnableLog { get; }

        private void Log(string msg)
        {
            if (!EnableLog)
            {
                return;
            }

            var oldForegroundColor = Console.ForegroundColor;
            var oldBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"\t [HttpClient Log / {SystemTime.Now.ToString("G")}] {msg}");

            Console.ForegroundColor = oldForegroundColor;
            Console.BackgroundColor = oldBackgroundColor;
        }



        public LoggingHttpMessageHandler(HttpMessageHandler innerHandler, bool enableLog = false) : base(innerHandler)
        {
            //TODO: 增加 ILoggerFactory? loggerFactory = null
            EnableLog = enableLog;
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Log($"Request: {request.Method} {request.RequestUri}");
            if (EnableLog && request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();
                var requestBody = await request.Content.ReadAsStringAsync();

                Log($"Request Body: {requestBody}");
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Log($"Response: {(int)response.StatusCode} {response.StatusCode}");

            if (EnableLog && response.Content != null)
            {
                await response.Content.LoadIntoBufferAsync();
                var responseBody = await response.Content.ReadAsStringAsync();
                Log($"Response Body: {responseBody}");
            }

            return response;
        }
    }

    public class BufferedHttpContent : HttpContent
    {
        private readonly byte[] _content;

        public BufferedHttpContent(byte[] content)
        {
            _content = content;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using var memoryStream = new MemoryStream(_content);
            await memoryStream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Length;
            return true;
        }
    }
}
