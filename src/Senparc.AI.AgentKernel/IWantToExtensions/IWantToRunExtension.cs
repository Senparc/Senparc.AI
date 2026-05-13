using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Kernels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.IWantToExtensions
{
    public static class IWantToRunExtension
    {
        //public static IWantToRun Run(this IWantToRun iWantToRun, AIFunction aIFunction, KernelArguments kernelArguments = null)
        //{
        //    AiKernel? kernel = iWantToRun.Kernel;
        //    kernel.RunAsync(aIFunction, kernelArguments).Wait();
        //}

        public static async Task<AgentResponse<string>> Run(this IWantToRun iWantToRun, string prompt)
        {
            AiKernel? kernel = iWantToRun.Kernel;
            return await kernel.RunAsync<string>(prompt);
        }

        public static async Task<AgentResponse<T>> Run<T>(this IWantToRun iWantToRun, string prompt)
        {
            AiKernel? kernel = iWantToRun.Kernel;
            return await kernel.RunAsync<T>(prompt);
        }
    }
}
