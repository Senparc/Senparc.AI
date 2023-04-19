using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Samples.Consoles;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using System.Reflection.Emit;

var configBuilder = new ConfigurationBuilder();
var appsettingsJsonFileName = SampleHelper.GetAppSettingsFile();//"appsettings.json"
configBuilder.AddJsonFile(appsettingsJsonFileName, false, false);
Console.WriteLine("完成 appsettings.json 添加");

var config = configBuilder.Build();
Console.WriteLine("完成 ServiceCollection 和 ConfigurationBuilder 初始化");

//更多绑定操作参见：https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2
var senparcSetting = new SenparcSetting();
config.GetSection("SenparcSetting").Bind(senparcSetting);

var senparcAiSetting = new Senparc.AI.SenparcAiSetting();
config.GetSection("SenparcAiSetting").Bind(senparcAiSetting);

var services = new ServiceCollection();
services.AddScoped<IAiHandler, SemanticAiHandler>();
services.AddScoped<ChatSample>();
services.AddSenparcGlobalServices(config);


var serviceProvider = services.BuildServiceProvider();

IRegisterService register = RegisterService.Start(senparcSetting)
              .UseSenparcGlobal()
              .UseSenparcAI(senparcAiSetting);


Console.WriteLine("请输入序号，开始对应功能测试：");
Console.WriteLine("[1] GPT对话机器人");
Console.WriteLine("[2] 训练 Embedding 任务");
var index = Console.ReadLine();
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
            //Embedding
            var embeddingSample = serviceProvider.GetRequiredService<EmbeddingSample>();
            await embeddingSample.RunAsync();
        }
        break;
    default:
        Console.WriteLine("序号错误，请重新开始！");
        break;
}