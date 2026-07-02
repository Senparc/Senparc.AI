using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles;

public class SampleSetting
{
    internal static string CurrentSettingKey { get; set; } = "Default";
    internal static bool EnableHttpClientLog { get; set; }

    internal static ISenparcAiSetting CurrentSetting => ModelSettingCatalog.Resolve(CurrentSettingKey);

    private enum SettingItems
    {
        Exit = 0,
        SelectModel = 1,
        ToggleHttpClientLog = 2,
        ResetAll = 3,
    }

    public void Run()
    {
        Console.Clear();
        Console.WriteLine("[Select a setting]");
        Console.WriteLine();
        Console.WriteLine($"* Enabling HttpClient logs may affect some API behavior. Current:{(EnableHttpClientLog ? "enabled" : "disabled")}");
        Console.WriteLine();

        var currentChoose = SampleHelper.ChooseItems<SettingItems>();

        var exit = false;
        switch (currentChoose)
        {
            case SettingItems.Exit:
                exit = true;
                break;
            case SettingItems.SelectModel:
                ChooseModel();
                break;
            case SettingItems.ToggleHttpClientLog:
                EnableHttpClientLog = !EnableHttpClientLog;
                Console.WriteLine($"[Debug] HttpClient logging is now {(EnableHttpClientLog ? "enabled" : "disabled")}");
                break;
            case SettingItems.ResetAll:
                CurrentSettingKey = "Default";
                EnableHttpClientLog = false;
                exit = true;
                break;
            default:
                Console.WriteLine("Select a valid option");
                Run();
                return;
        }

        if (!exit)
        {
            Run();
        }

        Console.Clear();
    }

    private static void ChooseModel()
    {
        Console.WriteLine("[Select a model configuration]");
        Console.WriteLine("Description: Default uses AiPlatform from appsettings. The remaining items are base *Keys configurations or Items subsets.");
        Console.WriteLine();

        var choices = ModelSettingCatalog.GetChoices();
        var labels = choices.Select(c => c.Label).ToArray();
        var chosen = SampleHelper.ChooseItems(labels);
        CurrentSettingKey = choices[chosen].Key;

        var resolved = CurrentSetting;
        Console.WriteLine($"[Debug] Current model configuration:{CurrentSettingKey} - {resolved.AiPlatform} - {resolved.Endpoint}");
        var chatModel = resolved.ModelName?.Chat;
        if (!chatModel.IsNullOrEmpty())
        {
            Console.WriteLine($"[Debug] Chat model:{chatModel}");
        }
    }
}
