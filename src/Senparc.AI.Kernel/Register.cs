using Senparc.AI.Kernel;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Kernel
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
        public static IRegisterService UseSenparcAI(this IRegisterService registerService, SenparcAiSetting senparcAiSetting = null)
        {
            senparcAiSetting ??= new SenparcAiSetting();
            return Senparc.AI.Register.UseSenparcAI(registerService, senparcAiSetting);
        }
    }
}
