using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// Final result API returned by an AI call
    /// </summary>
    public interface IAiResult
    {
        /// <summary>
        /// actual input parameter provided by the request, usually prompt
        /// </summary>
        string InputContent { get; set; }
        /// <summary>
        /// actual context provided by the request
        /// </summary>
        IAiContext InputContext { get; set; }
        /// <summary>
        /// output content(text)
        /// </summary>
        string OutputString { get; set; }
        /// <summary>
        /// most recent exception
        /// </summary>
        Exception? LastException { get; set; }
    }
}
