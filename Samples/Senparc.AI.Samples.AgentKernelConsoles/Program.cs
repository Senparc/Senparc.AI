using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.AgentKernel;
using Senparc.AI.Interfaces;
using Senparc.AI.Samples.AgentKernelConsoles;
using Senparc.AI.Samples.AgentKernelConsoles.Samples;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;

var configBuilder = new ConfigurationBuilder();
var appsettingsJsonFileName = SampleHelper.GetAppSettingsFile();
configBuilder.AddJsonFile(appsettingsJsonFileName, optional: false, reloadOnChange: false);
var config = configBuilder.Build();

var services = new ServiceCollection();
services.AddSenparcGlobalServices(config);
services.AddSenparcAI(config);
services.AddMemoryCache();

services.AddSingleton<SampleSetting>();
services.AddTransient<ChatSample>();
services.AddTransient<CompletionSample>();
services.AddTransient<EmbeddingSample>();
services.AddTransient<EmbeddingRagSample>();
services.AddTransient<ImageGenerateSample>();

var serviceProvider = services.BuildServiceProvider();

RegisterService.Start()
    .UseSenparcGlobal()
    .UseSenparcAI();

Start:
Console.WriteLine();
Console.WriteLine("Senparc.AI AgentKernel Sample 启动完毕");
Console.WriteLine("开源地址：https://github.com/Senparc/Senparc.AI");
Console.WriteLine("-----------------------");
Console.WriteLine($"当前模型：{SampleSetting.CurrentSettingKey} - {SampleSetting.CurrentSetting.AiPlatform} - {SampleSetting.CurrentSetting.Endpoint}");
Console.WriteLine($"当前 HttpClient 日志：{(SampleSetting.EnableHttpClientLog ? "开启" : "关闭")}");
Console.WriteLine($"当前向量库：{SampleSetting.CurrentSetting.VectorDB?.Type} {SampleSetting.CurrentSetting.VectorDB?.ConnectionString}");
Console.WriteLine("=======================");
Console.WriteLine();
Console.WriteLine("请输入序号，开始对应功能测试：");
Console.WriteLine("[0] 进入设置");
Console.WriteLine("[1] Chat 对话（AgentSession 多轮上下文）");
Console.WriteLine("[2] Completion 单次补全（无历史上下文）");
Console.WriteLine("[3] Embedding 与向量检索");
Console.WriteLine("[4] Dall·E 绘图");
Console.WriteLine("[5] Planner 任务计划");
Console.WriteLine("[6] PluginFromObject / Function Calling");
Console.WriteLine("[7] STT（Speech to Text）");
Console.WriteLine();

var index = Console.ReadLine();
Console.WriteLine();
await Console.Out.WriteLineAsync("任意时间输入 exit 可退出当前示例。");
Console.WriteLine();

switch (index)
{
    case "1":
        await serviceProvider.GetRequiredService<ChatSample>().RunAsync();
        break;
    case "2":
        await serviceProvider.GetRequiredService<CompletionSample>().RunAsync();
        break;
    case "3":
        Console.WriteLine("请选择 Embedding 子项：");
        Console.WriteLine("[1] 生成向量 + 相似度检索");
        Console.WriteLine("[2] RAG（TextSearchProvider）");
        var sub = Console.ReadLine();
        try
        {
            switch (sub)
            {
                case "1":
                    await serviceProvider.GetRequiredService<EmbeddingSample>().RunAsync();
                    break;
                case "2":
                    await serviceProvider.GetRequiredService<EmbeddingRagSample>().RunAsync();
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
        break;
    case "4":
        await serviceProvider.GetRequiredService<ImageGenerateSample>().RunAsync();
        break;
    case "5":
        await NotSupportedSample.RunAsync("Planner 任务计划");
        break;
    case "6":
        await NotSupportedSample.RunAsync("PluginFromObject / Function Calling");
        break;
    case "7":
        await NotSupportedSample.RunAsync("STT（Speech to Text）");
        break;
    case "0":
        serviceProvider.GetRequiredService<SampleSetting>().Run();
        break;
    default:
        Console.WriteLine("序号错误，请重新开始！");
        break;
}

Console.WriteLine("好，让我们重新开始！");
Console.WriteLine();
goto Start;
