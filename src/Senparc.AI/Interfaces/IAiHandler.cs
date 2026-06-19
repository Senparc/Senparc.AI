using Senparc.CO2NET;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Senparc.AI.Interfaces
{
    public interface IAiHandler
    { }

    public interface IAiHandler<TRequest, TResult, TContext> : IAiHandler
     where TRequest : IAiRequest<TContext>
        where TResult : IAiResult
        where TContext : IAiContext
    {
        /// <summary>
        /// run
        /// </summary>
        /// <param name="request">request data</param>
        /// <returns></returns>
        public TResult Run(TRequest request, ISenparcAiSetting? senparcAiSetting = null);

    }
}
