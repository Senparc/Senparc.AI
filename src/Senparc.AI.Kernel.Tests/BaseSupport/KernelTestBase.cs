using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Interfaces;
using Senparc.AI.Tests;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Tests.BaseSupport
{
    public class KernelTestBase : BaseTest
    {
        static Action<IRegisterService> RegisterAction = r =>
        {
            r.UseSenparcAI();
        };

        static Func<IConfigurationRoot, SenparcAiSetting> getSenparcAiSettingFunc = config =>
        {
            var senparcAiSetting = new SenparcAiSetting() { IsDebug = true };
            config.GetSection("SenparcAiSetting").Bind(_senparcAiSetting);
            return senparcAiSetting;
        };

        static Action<ServiceCollection> serviceAction = services =>
        {
            services.AddScoped<IAiHandler, SemanticAiHandler>();
        };

        public KernelTestBase() : base(RegisterAction, getSenparcAiSettingFunc, serviceAction)
        {
           

        }

    }
}
