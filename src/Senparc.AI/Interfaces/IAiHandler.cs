using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    public interface IAiHandler<T>
        where T : IAiResult
    {
        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <returns></returns>
        public T Run(IAiRequest request); 
    }
}
