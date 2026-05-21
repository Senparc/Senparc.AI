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
        退出 = 0,
        选择模型 = 1,
        自定义模型 = 2,
        设置背景颜色 = 3,
        设置字体颜色 = 4,
        启用或关闭HttpClient日志=5,
        全部重置=6,
    }

    private void SetModelAsync()
    {

    }

    /// <summary>
    /// 修改 Console 背景颜色
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private ConsoleColor SetBackgroundColorAsync()
    {
        Console.BackgroundColor = SampleHelper.ChooseItems<ConsoleColor>();
        return Console.BackgroundColor;
    }

    /// <summary>
    /// 修改 Console 字体颜色
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
        Console.WriteLine("[请选择设置内容]");
        Console.WriteLine();
        Console.WriteLine($"*请注意：设置 HttpClient 日之后，可能无法使用 Stream 方式获取返回内容。当前状态：{(EnableHttpClientLog?"启用":"关闭")}");
        Console.WriteLine();

        SettingItems currentChoose = SampleHelper.ChooseItems<SettingItems>();

        var exit = false;
        switch (currentChoose)
        {
            case SettingItems.退出:
                exit = true;
                break;
            case SettingItems.选择模型:
                Console.WriteLine("[请选择模型配置]");
                ChooseModel();
                break;
            case SettingItems.自定义模型:
                SetModelAsync();
                break;
            case SettingItems.设置背景颜色:
                BackgroundColor = SetBackgroundColorAsync();
                break;
            case SettingItems.设置字体颜色:
                ForceColor = SetForegroundColorAsync();
                break;
            case SettingItems.启用或关闭HttpClient日志:
                EnableHttpClientLog =!EnableHttpClientLog;
                Console.WriteLine($"HttpClient 日志已{(EnableHttpClientLog?"启用":"关闭")}");
                break;
            case SettingItems.全部重置:
                BackgroundColor = ConsoleColor.Black;
                ForceColor = ConsoleColor.White;
                CurrentSettingKey = "Default";
                exit = true;
                break;
            default:
                Console.Out.WriteLineAsync("请选择正确的选项");
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
        Console.WriteLine("说明：Default 使用 appsettings 中的 AiPlatform；其余项为各 *Keys 基础配置或 Items 子集。");
        Console.WriteLine();

        var choices = ModelSettingCatalog.GetChoices();
        var labels = choices.Select(c => c.Label).ToArray();
        var chosen = SampleHelper.ChooseItems(labels);
        CurrentSettingKey = choices[chosen].Key;

        var resolved = CurrentSetting;
        Console.WriteLine($"当前已选中模型配置：{CurrentSettingKey} - {resolved.AiPlatform} - {resolved.Endpoint}");
    }
}