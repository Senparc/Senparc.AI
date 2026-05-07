using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Entities;
using Senparc.AI.Kernel;
using Senparc.AI.Samples.Consoles.Samples;
using Senparc.AI.Samples.Consoles.Samples.Plugins;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
    .Build();

var setting = configuration.GetSection("SenparcAiSetting").Get<SenparcAiSetting>() ?? new SenparcAiSetting();
Senparc.AI.Config.SenparcAiSetting = setting;

var aiHandler = new SemanticAiHandler(setting);
var serviceProvider = new ServiceCollection().BuildServiceProvider();

while (true)
{
    Console.WriteLine("请输入序号，开始对应功能测试：");
    Console.WriteLine("[1] Chat 对话机器人");
    Console.WriteLine("[2] Completion 任务机器人");
    Console.WriteLine("[3] 执行 Embedding 任务（RAG）");
    Console.WriteLine("[4] Dall·E 绘图");
    Console.WriteLine("[5] Planner 任务计划");
    Console.WriteLine("[6] PluginFromObject 测试");
    Console.WriteLine("[7] STT（Speech to Text）测试");
    Console.WriteLine("[0] 退出");

    var index = Console.ReadLine();
    switch (index)
    {
        case "1":
            await new ChatSample(serviceProvider, aiHandler).RunAsync();
            break;
        case "2":
            await new CompletionSample(aiHandler).RunAsync();
            break;
        case "3":
            await new EmbeddingSample(aiHandler).RunRagAsync(serviceProvider);
            break;
        case "4":
            await new DallESample(aiHandler, serviceProvider).RunAsync();
            break;
        case "5":
            await new PlanSample(aiHandler).RunAsync();
            break;
        case "6":
            await new PluginFromObjectSample(aiHandler, serviceProvider).RunAsync();
            break;
        case "7":
            await new SttSample(serviceProvider, aiHandler).RunAsync();
            break;
        case "0":
            return;
    }
}