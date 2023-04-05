using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Senparc.AI.Interfaces
{
    public interface IAiHandler<TRequest, TResult, TAiContext, TContext>
     where TRequest : IAiRequest<TContext>
        where TResult : IAiResult
        where TAiContext : IAiContext<TContext>
        where TContext : class
    {
        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <returns></returns>
        public TResult Run(TRequest request);
    }
}
