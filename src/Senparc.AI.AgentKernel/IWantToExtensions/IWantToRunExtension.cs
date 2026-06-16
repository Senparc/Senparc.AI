using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Kernels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.IWantToExtensions
{
    public static class IWantToRunExtension
    {
        //public static IWantToRun Run(this IWantToRun iWantToRun, AIFunction aIFunction, AgentKernelArguments kernelArguments = null)
        //{
        //    AiKernel? kernel = iWantToRun.Kernel;
        //    kernel.RunAsync(aIFunction, kernelArguments).Wait();
        //}

        //public static Task<AgentResponse?> RunChat(this IWantToRun iWantToRun, string prompt, AgentSession? agentSession = null)
        //{
        //    return iWantToRun.Kernel.InvokeChatAsync(prompt, agentSession);
        //}

        //public static Task<AgentResponse<T>> RunChat<T>(this IWantToRun iWantToRun, string prompt, AgentSession? agentSession = null)
        //{
        //    return iWantToRun.Kernel.RunChatAsync<T>(prompt, agentSession);
        //}
    }
}
