using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Senparc.AI.Kernel.SparkAI
{
    public class OpenAIHttpClientHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UriBuilder uriBuilder;
            switch (request.RequestUri?.LocalPath) {
                case "/v1/chat/completions":
                    uriBuilder = new UriBuilder(request.RequestUri) {
                        // 这里是你要修改的 URL
                        Scheme = "http",
                        Host = "你的ip地址",
                        Port = 3000,
                        Path = "v1/chat/completions",
                    };
                    request.RequestUri = uriBuilder.Uri;
                    break;
            }

            // 接着，调用基类的 SendAsync 方法将你的修改后的请求发出去
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            int n = 0;
            while ((int)response.StatusCode == 500 && n < 10) {
                response = await base.SendAsync(request, cancellationToken);
                n++;
            }

            return response;
        }
    }

}
