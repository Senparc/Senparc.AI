using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Samples.Consoles;
using Senparc.AI.Samples.Consoles.Samples;
using Senparc.AI.Samples.Consoles.Samples.Plugins;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;

var configBuilder = new ConfigurationBuilder();
var appsettingsJsonFileName = SampleHelper.GetAppSettingsFile();//"appsettings.json"
configBuilder.AddJsonFile(appsettingsJsonFileName, false, false);
Console.WriteLine("appsettings.json added");

var config = configBuilder.Build();
Console.WriteLine("ServiceCollection and ConfigurationBuilder initialized");

var services = new ServiceCollection();

services.AddSenparcGlobalServices(config)
        .AddSenparcAI(config);

services.AddSingleton<SampleSetting>();
services.AddTransient<ChatSample>();
services.AddTransient<CompletionSample>();
services.AddTransient<EmbeddingSample>();
services.AddTransient<DallESample>();
services.AddTransient<PlanSample>();
services.AddTransient<PluginFromObjectSample>();
services.AddTransient<SttSample>();
services.AddTransient<TtsSample>();

services.AddScoped<IAiHandler, SemanticAiHandler>(s =>
{
    return new SemanticAiHandler(SampleSetting.CurrentSetting);
});

var serviceProvider = services.BuildServiceProvider();

IRegisterService register = RegisterService.Start()
              .UseSenparcGlobal()
              .UseSenparcAI();

Start:
Console.WriteLine();
Console.WriteLine("Senparc.AI Sample started");
Console.WriteLine("Open-source repository:https://github.com/Senparc/Senparc.AI");
Console.WriteLine("-----------------------");
Console.WriteLine($"Current model:{SampleSetting.CurrentSettingKey} - {SampleSetting.CurrentSetting.AiPlatform} - {SampleSetting.CurrentSetting.Endpoint}");
Console.WriteLine($"Current HttpClient logging switch:{(SampleSetting.EnableHttpClientLog ? "enabled" : "disabled")}");
Console.WriteLine($"Current vector database setting:{SampleSetting.CurrentSetting.VectorDB.Type} {SampleSetting.CurrentSetting.VectorDB.ConnectionString}");
Console.WriteLine("=======================");
Console.WriteLine();
Console.WriteLine("Enter a number to start the corresponding feature test:");
Console.WriteLine("[0] Settings");
Console.WriteLine("[1] Chat bot");
Console.WriteLine("[2] Completion bot");
Console.WriteLine("[3] Run Embedding task(RAG)");
Console.WriteLine("[4] DallE image generation(requires OpenAI or AzureOpenAI configuration)");
Console.WriteLine("[5] Planner task planning");
Console.WriteLine("[6] PluginFromObject test");
Console.WriteLine("[7] STT (Speech to Text) test");
Console.WriteLine("[8] TTS (Text to Speech) test");
Console.WriteLine();

var index = Console.ReadLine();
Console.WriteLine();

await Console.Out.WriteLineAsync("Enter exit at any time to leave the selection and restart.");
Console.WriteLine();

switch (index)
{
    case "1":
        {
            //Chat assistant sample
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
            Console.WriteLine("Select the Embedding test to run:");
            Console.WriteLine("[1] Standard Embedding + query");
            Console.WriteLine("[2] Retrieval-augmented generation(RAG)");
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
                            await embeddingSample.RunAsync();
                        }
                        break;
                    case "2":
                        {
                            await embeddingSample.RunRagAsync(serviceProvider);
                        }
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
    case "7":
        {
            //STT Sample
            var sttSample = serviceProvider.GetRequiredService<SttSample>();
            await sttSample.RunAsync();
        }
        break;
    case "8":
        {
            //TTS Sample
            var ttsSample = serviceProvider.GetRequiredService<TtsSample>();
            await ttsSample.RunAsync();
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
        Console.WriteLine("Invalid number. Restarting.");
        break;
}

Console.WriteLine("Restarting the sample menu.");
Console.WriteLine();
goto Start;
