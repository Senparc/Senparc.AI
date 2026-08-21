using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Interfaces
{
    /// <summary>
    /// Vector database configuration
    /// </summary>
    public class VectorDB
    {
        /// <summary>
        /// vector database type
        /// </summary>
        public VectorDBType Type { get; set; }
        /// <summary>
        /// connection string
        /// </summary>
        public string? ConnectionString { get; set; }
    }

    /// <summary>
    /// vector database type
    /// </summary>
    public enum VectorDBType
    {
        Memory = 0,
        HardDisk = 1,
        Redis = 2,
        Milvus = 3,
        Chroma = 4,
        PostgreSQL = 5,
        Sqlite = 6,
        SqlServer = 7,
        Qdrant = 8,
        Default = Memory,
    }
}
