using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SemanticKernel;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Kernels;

namespace Senparc.AI.AgentKernel.Helpers
{
    internal static class ExtensionHelper
    {
        /// <summary>
        /// setting AgentKernelArguments
        /// </summary>
        /// <param name="kernelArguments"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(this AgentKernelArguments kernelArguments, string key, object value)
        {
            kernelArguments[key] = value;
        }
    }
}
