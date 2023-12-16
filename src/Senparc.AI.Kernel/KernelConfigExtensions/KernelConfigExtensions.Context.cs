using Microsoft.SemanticKernel;
using Senparc.AI.Kernel.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Kernel.KernelConfigExtensions
{
    public static partial class KernelConfigExtensions
    {
        /// <summary>
        /// Create a new instance of a context, linked to the kernel internal state.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <returns>SK context</returns>
        public static (IWantToRun iWantToRun, KernelArguments arguments) CreateNewArguments(this IWantToRun iWantToRun)
        {
            //var helper = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticKernelHelper;
            //var kernel = helper.GetKernel();
            var context = new KernelArguments();// kernel.CreateNewContext();
            return (iWantToRun, context);
        }
    }
}
