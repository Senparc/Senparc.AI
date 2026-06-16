using Microsoft.SemanticKernel;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.AgentKernel.Kernels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        /// <summary>
        /// Create a new instance of a context, linked to the kernel internal state.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <returns>SK context</returns>
        public static (IWantToRun iWantToRun, AgentKernelArguments arguments) CreateNewArguments(this IWantToRun iWantToRun)
        {
            //var helper = iWantToRun.IWantToBuild.IWantToConfig.IWantTo.AgentKernelHelper;
            //var kernel = helper.GetKernel();
            var context = new AgentKernelArguments();// kernel.CreateNewContext();
            return (iWantToRun, context);
        }

        #region 运行阶段，或对生成后的 Kernel 进行补充设置

        #region 对上下文的管理

        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SenparcAiRequest SetTempContext(this SenparcAiRequest request, string key, string value)
        {
            request.TempAiArguments ??= new SenparcAiArguments();
            request.TempAiArguments.AgentKernelArguments.Set(key, value);
            return request;
        }

        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SenparcAiRequest SetStoredContext(this SenparcAiRequest request, string key, object value)
        {
            request.StoreAiArguments.AgentKernelArguments.Set(key, value);
            return request;
        }


        /// <summary>
        /// 获取上下文的值
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetTempArguments(this SenparcAiRequest request, string key, out object? value)
        {
            return request.TempAiArguments.AgentKernelArguments.TryGetValue(key, out value);
        }

        /// <summary>
        /// 获取上下文的值
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetStoredArguments(this SenparcAiRequest request, string key, out object? value)
        {
            return request.StoreAiArguments.AgentKernelArguments.TryGetValue(key, out value);
        }

        #endregion

        #endregion
    }
}
