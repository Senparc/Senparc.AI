using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Entities.Keys
{
    /// <summary>
    /// Keys 基类
    /// </summary>
    [Serializable]
    public class BaseKeys
    {
        /// <summary>
        /// 模型名称配置
        /// </summary>
        public ModelName ModelName { get; set; } = new ModelName();
    }

    /// <summary>
    /// 模型名称配置
    /// </summary>
    [Serializable]
    public class ModelName
    {
        /// <summary>
        /// 文本补全模型名称
        /// </summary>
        public string TextCompletion { get; set; }

        /// <summary>
        /// Chat模型名称
        /// </summary>
        public string Chat { get; set; }

        /// <summary>
        /// Embedding 模型名称
        /// </summary>
        public string Embedding { get; set; }

        /// <summary>
        /// 文生图 模型名称
        /// </summary>
        public string TextToImage { get; set; }

        /// <summary>
        /// 图生文 模型名称
        /// </summary>
        public string ImageToText { get; set; }
    }
}
