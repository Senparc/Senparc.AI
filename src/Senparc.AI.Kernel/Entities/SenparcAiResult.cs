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
        public virtual string InputContent { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual IAiContext InputContext { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual string Output { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual Exception? LastException { get; set; }

        public virtual IWantToRun IWantToRun { get; set; }

        public SenparcAiResult(IWantToRun iwantToRun, string? inputContent)
        {
            IWantToRun = iwantToRun;
            InputContent = inputContent;
        }

        public SenparcAiResult(IWantToRun iwantToRun, IAiContext inputContext)
        {
            IWantToRun = iwantToRun;
            InputContext = inputContext;
        }

    }

    public class SenaprcAiResult<T> : SenparcAiResult, IAiResult
    {
        public T Result { get; set; }
        public SenaprcAiResult(IWantToRun iWwantToRun, string inputContent)
            : base(iWwantToRun, inputContent)
        {
        }

        public SenaprcAiResult(IWantToRun iWwantToRun, IAiContext inputContext)
           : base(iWwantToRun, inputContext)
        {
        }
    }

    public class SenaprcContentAiResult : SenaprcAiResult<SKContext>, IAiResult
    {
        public SKContext Result { get; set; }
        public SenaprcContentAiResult(IWantToRun iWwantToRun, string inputContent)
             : base(iWwantToRun, inputContent)
        {
        }

        public SenaprcContentAiResult(IWantToRun iWwantToRun, IAiContext inputContext)
           : base(iWwantToRun, inputContext)
        {
        }


    }
}