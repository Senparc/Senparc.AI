using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    public interface IAiContext
    {
        /// <summary>
        /// 基础上下文
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> Context { get; set; }
        //bool StoreToContainer { get; set; }
    }

    public interface IAiContext<T> : IAiContext
        where T : class, IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// 扩展类型的上下文
        /// </summary>
        T ExtendContext { get; set; }
    }
}
