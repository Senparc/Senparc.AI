using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// AI 调用返回的最终结果接口
    /// </summary>
    public interface IAiResult
    {
        /// <summary>
        /// 请求实际提供的输入参数，一般为 prompt
        /// </summary>
        string InputContent { get; set; }
        /// <summary>
        /// 请求实际提供的上下文
        /// </summary>
        IAiContext InputContext { get; set; }
        /// <summary>
        /// 输出内容
        /// </summary>
        string Output { get; set; }
        /// <summary>
        /// 最近一个异常
        /// </summary>
        Exception? LastException { get; set; }
    }
}
