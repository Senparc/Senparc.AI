using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Samples.Consoles;
using Senparc.AI.Samples.Consoles.Samples;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;

var configBuilder = new ConfigurationBuilder();
var appsettingsJsonFileName = SampleHelper.GetAppSettingsFile();//"appsettings.json"
configBuilder.AddJsonFile(appsettingsJsonFileName, false, false);
Console.WriteLine("完成 appsettings.json 添加");

var config = configBuilder.Build();
Console.WriteLine("完成 ServiceCollection 和 ConfigurationBuilder 初始化");

//更多绑定操作参见：https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2
var senparcSetting = new SenparcSetting();
config.GetSection("SenparcSetting").Bind(senparcSetting);

var senparcAiSetting = new Senparc.AI.Kernel.SenparcAiSetting();
config.GetSection("SenparcAiSetting").Bind(senparcAiSetting);

var services = new ServiceCollection();
services.AddSenparcGlobalServices(config);

services.AddScoped<IAiHandler, SemanticAiHandler>();
services.AddScoped<ChatSample>();
services.AddScoped<EmbeddingSample>();
services.AddScoped<DallESample>();
services.AddScoped<PlanSample>();


var serviceProvider = services.BuildServiceProvider();

IRegisterService register = RegisterService.Start(senparcSetting)
              .UseSenparcGlobal()
              .UseSenparcAI(senparcAiSetting);


Console.WriteLine("启动完毕，当前接口：" + Senparc.AI.Config.SenparcAiSetting.AiPlatform);
Console.WriteLine("=======================");
Console.WriteLine();
Console.WriteLine("请输入序号，开始对应功能测试：");
Console.WriteLine("[1] GPT对话机器人");
Console.WriteLine("[2] 训练 Embedding 任务");
Console.WriteLine("[3] Dall·E 绘图（需要配置 OpenAI）");
Console.WriteLine("[4] Planner 任务计划");

var index = Console.ReadLine();
Console.WriteLine();

switch (index)
{
    case "1":
        {
            //对话机器人 Sample
            var chatSample = serviceProvider.GetRequiredService<ChatSample>();
            await chatSample.RunAsync();
        }
        break;
    case "2":
        {
            //Embedding Sample
            Console.WriteLine("请输入需要，进入对应 Embedding 测试：");
            Console.WriteLine("[1] 普通信息（Information）");
            Console.WriteLine("[2] 引用信息（Reference）");
            index = Console.ReadLine();
            Console.WriteLine();
            try
            {
                //Embedding
                var embeddingSample = serviceProvider.GetRequiredService<EmbeddingSample>();

                switch (index)
                {
                    case "1":
                        {
                            await embeddingSample.RunAsync(isReference: false);
                        }
                        break;
                    case "2":
                        {
                            await embeddingSample.RunAsync(isReference: true);
                        }
                        break;
                    default:
                        Console.WriteLine("序号错误，请重新开始！");
                        break;
                }
            }
            catch (Exception)
            {
                goto case "2";
            }
        }
        break;
    case "3":
        {
            //DallE Sample
            var dallESample = serviceProvider.GetRequiredService<DallESample>();
            await dallESample.RunAsync();
        }
        break;
    case "4":
        {
            //Plan Sample
            var pnalSample = serviceProvider.GetRequiredService<PlanSample>();
            await pnalSample.RunAsync();
        }
        break;
    default:
        Console.WriteLine("序号错误，请重新开始！");
        break;
}