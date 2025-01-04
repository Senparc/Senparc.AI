using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.HttpMessageHandlers
{
    /// <summary>
    /// 重定向 HttpMessageHandler，目前主要服务于 OpenAI 的 Proxy
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
            //目前对OpenAI开放第三方代理API设置
            if(_senparcAiSetting.AiPlatform == AiPlatform.OpenAI && !_senparcAiSetting.OpenAIEndpoint.IsNullOrEmpty())
            {
                request.RequestUri = new UriBuilder(request.RequestUri!) { Host = _senparcAiSetting.OpenAIEndpoint }.Uri;
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
