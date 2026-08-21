using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.HttpMessageHandlers
{
    /// <summary>
    /// Redirect HttpMessageHandler. Currently mainly serves OpenAI proxies.
    /// </summary>
    public class RedirectingHttpMessageHandler: DelegatingHandler
    {
        private ISenparcAiSetting _senparcAiSetting;

        public RedirectingHttpMessageHandler(HttpMessageHandler innerHandler, ISenparcAiSetting senparcAiSetting) : base(innerHandler)
        {
            _senparcAiSetting = senparcAiSetting;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //Currently enables third-party proxy API settings for OpenAI
            if(_senparcAiSetting.AiPlatform == AiPlatform.OpenAI && !_senparcAiSetting.OpenAIEndpoint.IsNullOrEmpty())
            {
                request.RequestUri = new UriBuilder(request.RequestUri!) { Host = _senparcAiSetting.OpenAIEndpoint }.Uri;
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
