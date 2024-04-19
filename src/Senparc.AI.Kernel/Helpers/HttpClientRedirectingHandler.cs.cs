using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Helpers
{
    internal class HttpClientRedirectingHandler : DelegatingHandler
    {
        private string _endpoint;

        public HttpClientRedirectingHandler(string endpoint) :base(new HttpClientHandler())
        {
            _endpoint = endpoint;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.RequestUri = new UriBuilder(request.RequestUri!) { Host = _endpoint }.Uri;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
