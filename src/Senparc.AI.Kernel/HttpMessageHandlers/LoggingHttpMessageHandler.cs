using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.HttpMessageHandlers
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
                var contentStream = await request.Content.ReadAsStreamAsync();
                contentStream.Seek(0, SeekOrigin.Begin);
                using var streamReader = new StreamReader(contentStream);
                var requestBody = await streamReader.ReadToEndAsync();
                contentStream.Seek(0, SeekOrigin.Begin); // 重置流位置 

                Log($"Request Body: {requestBody}");
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Log($"Response: {(int)response.StatusCode} {response.StatusCode}");

            if (EnableLog && response.Content != null)
            {
                // 缓冲响应内容  
                await response.Content.LoadIntoBufferAsync();

                var contentStream = await response.Content.ReadAsStreamAsync();
                string responseBody;
                using (var streamReader = new StreamReader(contentStream))
                {
                    responseBody = await streamReader.ReadToEndAsync();
                    Log($"Response Body: {responseBody}");

                    // 创建一个新的 MemoryStream，以防止 ObjectDisposedException  
                    contentStream.Seek(0, SeekOrigin.Begin);
                    var memoryStream = new MemoryStream();
                    await contentStream.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    response.Content = new StreamContent(memoryStream);
                }
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
