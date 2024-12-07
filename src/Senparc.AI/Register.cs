using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI
{
    /// <summary>
    /// 注册 Senparc.AI
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcAICore(this IRegisterService registerService)
        {
           
            return registerService;
        }
    }
}
