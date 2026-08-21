using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Samples.Consoles;

public class SampleSetting
{
    internal static string CurrentSettingKey { get; set; } = "Default";
    internal static bool EnableHttpClientLog { get; set; } = false;

    internal static ISenparcAiSetting CurrentSetting => ModelSettingCatalog.Resolve(CurrentSettingKey);


    private enum SettingItems
    {
        Exit = 0,
        SelectModel = 1,
        CustomModel = 2,
        SetBackgroundColor = 3,
        SetForegroundColor = 4,
        ToggleHttpClientLog=5,
        ResetAll=6,
    }

    private void SetModelAsync()
    {

    }

    /// <summary>
    /// Change Console background color
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private ConsoleColor SetBackgroundColorAsync()
    {
        Console.BackgroundColor = SampleHelper.ChooseItems<ConsoleColor>();
        return Console.BackgroundColor;
    }

    /// <summary>
    /// Change Console foreground color
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private ConsoleColor SetForegroundColorAsync()
    {
        var color = SampleHelper.ChooseItems<ConsoleColor>();
        //Console.WriteLine("Color:" + color);
        Console.ForegroundColor = color;
        return Console.ForegroundColor;
    }

    internal static ConsoleColor BackgroundColor = ConsoleColor.Black;
    internal static ConsoleColor ForceColor = ConsoleColor.White;

    public void RunAsync()
    {
        Console.Clear();
        Console.WriteLine("[Select a setting]");
        Console.WriteLine();
        Console.WriteLine($"*Note: after enabling HttpClient logging, Stream mode may not be able to get returned content. Current status:{(EnableHttpClientLog?"enabled":"disabled")}");
        Console.WriteLine();

        SettingItems currentChoose = SampleHelper.ChooseItems<SettingItems>();

        var exit = false;
        switch (currentChoose)
        {
            case SettingItems.Exit:
                exit = true;
                break;
            case SettingItems.SelectModel:
                Console.WriteLine("[Select a model configuration]");
                ChooseModel();
                break;
            case SettingItems.CustomModel:
                SetModelAsync();
                break;
            case SettingItems.SetBackgroundColor:
                BackgroundColor = SetBackgroundColorAsync();
                break;
            case SettingItems.SetForegroundColor:
                ForceColor = SetForegroundColorAsync();
                break;
            case SettingItems.ToggleHttpClientLog:
                EnableHttpClientLog =!EnableHttpClientLog;
                Console.WriteLine($"HttpClient logging is now {(EnableHttpClientLog?"enabled":"disabled")}");
                break;
            case SettingItems.ResetAll:
                BackgroundColor = ConsoleColor.Black;
                ForceColor = ConsoleColor.White;
                CurrentSettingKey = "Default";
                exit = true;
                break;
            default:
                Console.Out.WriteLineAsync("Select a valid option");
                RunAsync();
                break;
        }


        Console.BackgroundColor = BackgroundColor;
        Console.ForegroundColor = ForceColor;

        if (!exit)
        {
            RunAsync();
        }

        Console.Clear();

    }

    private void ChooseModel()
    {
        Console.WriteLine("Description: Default uses AiPlatform from appsettings. The remaining items are base *Keys configurations or Items subsets.");
        Console.WriteLine();

        var choices = ModelSettingCatalog.GetChoices();
        var labels = choices.Select(c => c.Label).ToArray();
        var chosen = SampleHelper.ChooseItems(labels);
        CurrentSettingKey = choices[chosen].Key;

        var resolved = CurrentSetting;
        Console.WriteLine($"Selected model configuration:{CurrentSettingKey} - {resolved.AiPlatform} - {resolved.Endpoint}");
    }
}