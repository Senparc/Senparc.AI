using System;
using System.Collections.Generic;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// IAiContext
    /// </summary>
    public interface IAiContext
    {
        /// <summary>
        /// 基础上下文
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> Context { get; set; }
        //bool StoreToContainer { get; set; }
    }

    /// <summary>
    /// IAiContext with Genericity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAiContext<T> : IAiContext
        where T : class, IEnumerable<KeyValuePair<string, string>>
    {
        // /// <summary>
        // /// 扩展类型的上下文
        // /// </summary>
        // [Obsolete("请使用 ContextVariables", true)]
        // T ExtendContext { get; set; }
        /// <summary>
        /// 扩展类型的上下文
        /// </summary>
        T ContextVariables { get; set; }
    }
}
