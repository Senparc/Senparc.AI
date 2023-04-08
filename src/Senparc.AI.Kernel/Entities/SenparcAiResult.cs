using Microsoft.SemanticKernel.Orchestration;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Handlers;
using System;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// Senparc.AI.Kernel 模块的 AI 接口返回信息
    /// </summary>
    public class SenparcAiResult : IAiResult
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual string Input { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual string Output { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual Exception? LastException { get; set; }

        public virtual IWantToRun IWantToRun { get; set; }

        public SenparcAiResult(IWantToRun iwantToRun)
        {
            IWantToRun = iwantToRun;
        }


    }

    public class SenaprcAiResult<T> : SenparcAiResult, IAiResult
    {
        public T Result { get; set; }
        public SenaprcAiResult(IWantToRun iWwantToRun) : base(iWwantToRun)
        {
        }
    }

    public class SenaprcContentAiResult : SenaprcAiResult<SKContext>, IAiResult
    {
        public SKContext Result { get; set; }
        public SenaprcContentAiResult(IWantToRun iWwantToRun) : base(iWwantToRun)
        {
        }


    }
}