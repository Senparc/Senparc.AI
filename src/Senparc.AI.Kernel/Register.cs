using Microsoft.Extensions.Configuration;
using Senparc.AI.Exceptions;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// 注册 Senparc.AI
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// Get AppSettings file name.
        /// </summary>
        /// <returns></returns>
        public static string GetAppSettingsFile()
        {
            if (File.Exists("appsettings.Development.json"))
            {
                Console.WriteLine("use appsettings.Development.json");
                return "appsettings.Development.json";
            }

            Console.WriteLine("use appsettings.json");

            return "appsettings.json";
        }

        /// <summary>
        /// 注册 Senparc.AI
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSenparcAI(this IServiceCollection services, IConfiguration config, ISenparcAiSetting senparcAiSetting = null)
        {
            if (senparcAiSetting == null)
            {
                senparcAiSetting = new Senparc.AI.Kernel.SenparcAiSetting();
                config.GetSection("SenparcAiSetting").Bind(senparcAiSetting);
            }

            if (Config.SenparcAiSetting == null || Config.SenparcAiSetting.AiPlatform == AiPlatform.UnSet)
            {
                //只在原始配置未设置的时候机型覆盖
                Senparc.AI.Config.SenparcAiSetting = senparcAiSetting;
            }
            services.AddScoped<ISenparcAiSetting>(s => Config.SenparcAiSetting);
            services.AddScoped<IAiHandler, SemanticAiHandler>();

            return services;
        }

        /// <summary>
        /// 注册并运行 Senparc.AI
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcAI(this IRegisterService registerService)
        {
            return Senparc.AI.Register.UseSenparcAICore(registerService);
        }
    }
}
