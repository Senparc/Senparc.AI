using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// 向量数据库配置
    /// </summary>
    public class VectorDB
    {
        /// <summary>
        /// 向量数据库类型
        /// </summary>
        public VectorDBType Type { get; set; }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string? ConnectionString { get; set; }
    }

    /// <summary>
    /// 向量数据库类型
    /// </summary>
    public enum VectorDBType
    {
        Memory,
        HardDisk,
        Redis,
        Milvus,
        Chroma,
        PostgreSQL,
        Sqlite,
        SqlServer,
        Qdrant,
        Default = Memory,
    }
}
