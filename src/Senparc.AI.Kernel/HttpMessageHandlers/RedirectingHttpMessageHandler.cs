using Senparc.CO2NET.Extensions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.HttpMessageHandlers
{
    public class RedirectingHttpMessageHandler: DelegatingHandler
    {
        private string? _endpoint;
        private AiPlatform _aiPlatform;

        public RedirectingHttpMessageHandler(HttpMessageHandler innerHandler, AiPlatform aiPlatform, string? endpoint) : base(innerHandler)
        {
            _endpoint = endpoint;
            _aiPlatform = aiPlatform;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //目前对OpenAI开放第三方代理API设置
            if(_aiPlatform == AiPlatform.OpenAI && !_endpoint.IsNullOrEmpty())
            {
                request.RequestUri = new UriBuilder(request.RequestUri!) { Host = _endpoint }.Uri;
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
