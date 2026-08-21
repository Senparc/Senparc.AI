using Senparc.AI.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// request data API
    /// </summary>
    public interface IAiRequest<TContext> where TContext : IAiContext
    {
        /// <summary>
        /// user identifier
        /// </summary>
        string UserId { get; set; }
        ///// <summary>
        ///// Called model name
        ///// </summary>
        //string ModelName { get; set; }
        /// <summary>
        /// request content, such as prompt
        /// </summary>
        string RequestContent { get; set; }
        /// <summary>
        /// Parameter definition
        /// </summary>
        PromptConfigParameter ParameterConfig { get; set; }
        /// <summary>
        /// context
        /// </summary>
        TContext TempAiArguments { get; set; }
    }
}
