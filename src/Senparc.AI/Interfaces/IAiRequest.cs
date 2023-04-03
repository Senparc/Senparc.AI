using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// 请求数据接口
    /// </summary>
    public interface IAiRequest
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        string UserId { get; set; }
        /// <summary>
        /// 调用模型的名称
        /// </summary>
        string ModelName { get; set; }
        /// <summary>
        /// 请求内容，如 prompt
        /// </summary>
        string RequestContent { get; set; }
    }
}
