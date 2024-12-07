using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SemanticKernel;

namespace Senparc.AI.Kernel.Helpers
{
    internal static class ExtensionHelper
    {
        /// <summary>
        /// 设置 KernelArguments
        /// </summary>
        /// <param name="kernelArguments"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(this KernelArguments kernelArguments, string key, object value)
        {
            kernelArguments[key] = value;
        }
    }
}
