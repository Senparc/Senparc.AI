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
        IDictionary<string, object?> Context { get; set; }
        //bool StoreToContainer { get; set; }
    }

    /// <summary>
    /// IAiContext with Genericity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAiContext<T> : IAiContext
        where T : class, IDictionary<string, object?>
    {
        // /// <summary>
        // /// 扩展类型的上下文
        // /// </summary>
        // [Obsolete("请使用 ContextVariables", true)]
        // T ExtendContext { get; set; }
        /// <summary>
        /// 扩展类型的上下文
        /// </summary>
        T KernelArguments { get; set; }
    }
}
