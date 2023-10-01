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
        public static string Default_TextCompletion = null;
        public static string Default_TextEmbedding = null;

        static Action<IRegisterService> RegisterAction = r =>
        {
            r.UseSenparcAI(BaseTest._senparcAiSetting);
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
            switch (Senparc.AI.Config.SenparcAiSetting.AiPlatform)
            {
                case AiPlatform.HuggingFace:
                    Default_TextCompletion = "chatglm2";
                    Default_TextEmbedding = "chatglm2";
                    break;
                case AiPlatform.UnSet:
                case AiPlatform.None:
                case AiPlatform.OpenAI:
                case AiPlatform.NeuCharOpenAI:
                case AiPlatform.AzureOpenAI:
                default:
                    Default_TextCompletion = "text-davinci-003";
                    Default_TextEmbedding = "text-embedding-ada-002";
                    break;
            }

        }

    }
}
