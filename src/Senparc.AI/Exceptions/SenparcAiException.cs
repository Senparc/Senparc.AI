using Senparc.AI.Trace;
using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Exceptions
{
    /// <summary>
    /// SenparcAI exception
    /// </summary>
    public class SenparcAiException : BaseException
    {
        public string ModelName { get; set; }
        public string EndpointUrl { get; set; }

        public SenparcAiException(string message, bool logged = false) : base(message, logged)
        {
        }

        /// <summary>
        /// WeixinException
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="inner">inner exception information</param>
        /// <param name="logged">Whether Trace has already recorded the log. If not, WeixinException records a summary.</param>
        public SenparcAiException(string message, Exception inner, bool logged = false)
            : base(message, inner, true/* mark as log recorded */)
        {
            if (!logged)
            {
                SenparcAiTrace.SenparcAiExceptionLog(this);
            }
        }
    }
}
