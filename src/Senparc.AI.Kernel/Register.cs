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
    /// Register Senparc.AI
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
        /// Register Senparc.AI
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
                //Only override the model when the original configuration is not set
                Senparc.AI.Config.SenparcAiSetting = senparcAiSetting;
            }
            services.AddScoped<ISenparcAiSetting>(s => Config.SenparcAiSetting);
            services.AddScoped<IAiHandler, SemanticAiHandler>();

            return services;
        }

        /// <summary>
        /// Register and run Senparc.AI
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
