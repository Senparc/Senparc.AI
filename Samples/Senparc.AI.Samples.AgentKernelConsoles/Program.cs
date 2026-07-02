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
services.AddSingleton<IConfiguration>(config);

services.AddSingleton<SampleSetting>();
services.AddTransient<ChatSample>();
services.AddTransient<CompletionSample>();
services.AddTransient<EmbeddingSample>();
services.AddTransient<EmbeddingRagSample>();
services.AddTransient<ImageGenerateSample>();
services.AddTransient<SttSample>();
services.AddTransient<TtsSample>();
services.AddTransient<McpSample>();

var serviceProvider = services.BuildServiceProvider();

RegisterService.Start()
    .UseSenparcGlobal()
    .UseSenparcAI();

Start:
Console.WriteLine();
Console.WriteLine("Senparc.AI AgentKernel Sample started");
Console.WriteLine("Open-source repository:https://github.com/Senparc/Senparc.AI");
Console.WriteLine("-----------------------");
Console.WriteLine($"Current model:{SampleSetting.CurrentSettingKey} - {SampleSetting.CurrentSetting.AiPlatform} - {SampleSetting.CurrentSetting.Endpoint}");
Console.WriteLine($"Current HttpClient logging:{(SampleSetting.EnableHttpClientLog ? "enabled" : "disabled")}");
Console.WriteLine($"Current vector store:{SampleSetting.CurrentSetting.VectorDB?.Type} {SampleSetting.CurrentSetting.VectorDB?.ConnectionString}");
Console.WriteLine("=======================");
Console.WriteLine();
Console.WriteLine("Enter a number to start the corresponding feature test:");
Console.WriteLine("[0] Settings");
Console.WriteLine("[1] Chat conversation(AgentSession multi-turn context)");
Console.WriteLine("[2] Completion single-turn completion(without history context)");
Console.WriteLine("[3] Embedding and vector retrieval");
Console.WriteLine("[4] GPT-Image-2 image generation");
Console.WriteLine("[5] Planner task planning");
Console.WriteLine("[6] PluginFromObject / Function Calling");
Console.WriteLine("[7] STT(Speech to Text)");
Console.WriteLine("[8] TTS (Text to Speech)");
Console.WriteLine("[9] MCP(Hosted MCP Server Tool)");
Console.WriteLine();

var index = Console.ReadLine();
Console.WriteLine();
await Console.Out.WriteLineAsync("Enter exit at any time to leave the current sample.");
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
        Console.WriteLine("Select an Embedding sub-item:");
        Console.WriteLine("[1] Generate vectors and run similarity search");
        Console.WriteLine("[2] RAG(TextSearchProvider)");
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
                    Console.WriteLine("Invalid number. Restarting.");
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
        await NotSupportedSample.RunAsync("Planner task planning");
        break;
    case "6":
        await NotSupportedSample.RunAsync("PluginFromObject / Function Calling");
        break;
    case "7":
        await serviceProvider.GetRequiredService<SttSample>().RunAsync();
        break;
    case "8":
        await serviceProvider.GetRequiredService<TtsSample>().RunAsync();
        break;
    case "9":
        await serviceProvider.GetRequiredService<McpSample>().RunAsync();
        break;
    case "0":
        serviceProvider.GetRequiredService<SampleSetting>().Run();
        break;
    default:
        Console.WriteLine("Invalid number. Restarting.");
        break;
}

Console.WriteLine("Restarting the sample menu.");
Console.WriteLine();
goto Start;
