using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Kernel;
using Senparc.AI.Samples.Consoles;
using Senparc.AI.Samples.Consoles.Samples;
using Senparc.AI.Samples.Consoles.Samples.Plugins;
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

var services = new ServiceCollection();

services.AddSenparcGlobalServices(config)
        .AddSenparcAI(config);

services.AddTransient<ChatSample>();
services.AddTransient<CompletionSample>();
services.AddTransient<EmbeddingSample>();
services.AddTransient<DallESample>();
services.AddTransient<PlanSample>();
services.AddTransient<SampleSetting>();
services.AddTransient<PluginFromObjectSample>();

var serviceProvider = services.BuildServiceProvider();

IRegisterService register = RegisterService.Start(senparcSetting)
              .UseSenparcGlobal()
              .UseSenparcAI();

Start:
Console.WriteLine();
Console.WriteLine("Senparc.AI Sample 启动完毕");
Console.WriteLine("开源地址：https://github.com/Senparc/Senparc.AI");
Console.WriteLine("-----------------------");
Console.WriteLine($"当前模型：{SampleSetting.CurrentSettingKey} - {SampleSetting.CurrentSetting.AiPlatform} - {SampleSetting.CurrentSetting.Endpoint}");
Console.WriteLine($"当前 HttpClient 日志开关：{(SampleSetting.EnableHttpClientLog ? "开启" : "关闭")}");
Console.WriteLine("=======================");
Console.WriteLine();
Console.WriteLine("请输入序号，开始对应功能测试：");
Console.WriteLine("[0] 进入设置");
Console.WriteLine("[1] Chat 对话机器人");
Console.WriteLine("[2] Completion 任务机器人");
Console.WriteLine("[3] 训练 Embedding 任务");
Console.WriteLine("[4] Dall·E 绘图（需要配置 OpenAI 或 AzureOpenAI）");
Console.WriteLine("[5] Planner 任务计划");
Console.WriteLine("[6] PluginFromObject 测试");
Console.WriteLine();

var index = Console.ReadLine();
Console.WriteLine();

await Console.Out.WriteLineAsync("任意时间输入 exit 退出选择并重新开始。");
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
            //Completion Sample
            var completionSample = serviceProvider.GetRequiredService<CompletionSample>();
            await completionSample.RunAsync();
        }
        break;
    case "3":
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                goto case "3";
            }
        }
        break;
    case "4":
        {
            //DallE Sample
            var dallESample = serviceProvider.GetRequiredService<DallESample>();
            await dallESample.RunAsync();
        }
        break;
    case "5":
        {
            //Plan Sample
            var planSample = serviceProvider.GetRequiredService<PlanSample>();
            await planSample.RunAsync();
        }
        break;
    case "6":
        {
            //Function Sample
            var functionSample = serviceProvider.GetRequiredService<PluginFromObjectSample>();
            await functionSample.RunAsync();
        }
        break;
    case "0":
        {
            //Setting
            var setting = serviceProvider.GetRequiredService<SampleSetting>();
            setting.RunAsync();
        }
        break;
    default:
        Console.WriteLine("序号错误，请重新开始！");
        break;
}

Console.WriteLine("好，让我们重新开始！");
Console.WriteLine();
goto Start;