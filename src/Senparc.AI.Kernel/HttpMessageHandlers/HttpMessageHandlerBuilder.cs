using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Senparc.AI.Kernel.HttpMessageHandlers
{
    public class HttpMessageHandlerBuilder
    {
        private readonly List<HttpMessageHandler> _handlers;

        public HttpMessageHandlerBuilder()
        {
            _handlers = new List<HttpMessageHandler>();
        }

        /// <summary>
        /// Register HttpMessageHandler. Earlier registrations are called first.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public HttpMessageHandlerBuilder Add(DelegatingHandler handler)
        {
            _handlers.Add(handler);
            return this;
        }

        /// <summary>
        /// Create the final HttpMessageHandler
        /// </summary>
        /// <returns></returns>
        public DelegatingHandler Build()
        {
            DelegatingHandler wrapper = new ConcreteHttpMessageHandler();
            DelegatingHandler current = wrapper;
            HttpMessageHandler last = _handlers.Last();

            foreach (DelegatingHandler handler in _handlers)
            {
                if (handler == last)
                {
                    current.InnerHandler = handler;
                }
                else
                {
                    current.InnerHandler = handler;
                    current = handler;
                }
            }
            return wrapper;
        }

        public class ConcreteHttpMessageHandler : DelegatingHandler { }
    }
}
