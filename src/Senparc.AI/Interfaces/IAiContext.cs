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
        /// base context
        /// </summary>
        IDictionary<string, object?> Context { get; set; }
        //bool StoreToContainer { get; set; }
    }

    /// <summary>
    /// IAiContext with Genericity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAiContext<T> : IAiContext
        where T : IDictionary<string, object?>
    {
        // /// <summary>
        // /// context for the extension type
        // /// </summary>
        // [Obsolete("please use ContextVariables", true)]
        // T ExtendContext { get; set; }
        /// <summary>
        /// context for the extension type
        /// </summary>
        T KernelArguments { get; set; }
    }

    public interface IAiAgentContext<T> : IAiContext<T>
        where T : ISenparcKernelArguments
    {
        // /// <summary>
        // /// context for the extension type
        // /// </summary>
        // [Obsolete("please use ContextVariables", true)]
        // T ExtendContext { get; set; }
        /// <summary>
        /// context for the extension type
        /// </summary>
        T AgentKernelArguments { get; set; }
    }
}
