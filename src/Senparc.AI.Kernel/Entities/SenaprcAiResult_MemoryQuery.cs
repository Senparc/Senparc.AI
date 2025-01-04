using Microsoft.SemanticKernel.Memory;
using Senparc.AI.Exceptions;
using Senparc.AI.Kernel.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable SKEXP0003

namespace Senparc.AI.Kernel.Entities
{
    /// <summary>
    /// MemoryQueryResult
    /// </summary>
    public class SenaprcAiResult_MemoryQuery : SenparcAiResult
    {
        public override string OutputString { get => throw new SenparcAiException("请从 MemoryQueryResult 获取"); set => throw new SenparcAiException("当前方法内无法设置"); }
        /// <summary>
        /// Whether the source data used to calculate embeddings are stored in the local
        /// storage provider or is available through an external service, such as web site, MS Graph, etc.
        /// </summary>
        public IAsyncEnumerable<MemoryQueryResult> MemoryQueryResult { get; set; }
        /// <summary>
        /// Search relevance, from 0 to 1, where 1 means perfect match.
        /// </summary>
        public double Relevance { get; set; }

        public SenaprcAiResult_MemoryQuery(IWantToRun iwantToRun,string inputContent) 
            : base(iwantToRun, inputContent)
        {
        }

    }
}
