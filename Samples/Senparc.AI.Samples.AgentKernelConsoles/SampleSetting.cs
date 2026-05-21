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
        退出 = 0,
        选择模型 = 1,
        启用或关闭HttpClient日志 = 2,
        全部重置 = 3,
    }

    public void Run()
    {
        Console.Clear();
        Console.WriteLine("[请选择设置内容]");
        Console.WriteLine();
        Console.WriteLine($"* 启用 HttpClient 日志后可能影响部分接口行为。当前：{(EnableHttpClientLog ? "开启" : "关闭")}");
        Console.WriteLine();

        var currentChoose = SampleHelper.ChooseItems<SettingItems>();

        var exit = false;
        switch (currentChoose)
        {
            case SettingItems.退出:
                exit = true;
                break;
            case SettingItems.选择模型:
                ChooseModel();
                break;
            case SettingItems.启用或关闭HttpClient日志:
                EnableHttpClientLog = !EnableHttpClientLog;
                Console.WriteLine($"[调试] HttpClient 日志已{(EnableHttpClientLog ? "启用" : "关闭")}");
                break;
            case SettingItems.全部重置:
                CurrentSettingKey = "Default";
                EnableHttpClientLog = false;
                exit = true;
                break;
            default:
                Console.WriteLine("请选择正确的选项");
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
        Console.WriteLine("[请选择模型配置]");
        Console.WriteLine("说明：Default 使用 appsettings 中的 AiPlatform；其余项为各 *Keys 基础配置或 Items 子集。");
        Console.WriteLine();

        var choices = ModelSettingCatalog.GetChoices();
        var labels = choices.Select(c => c.Label).ToArray();
        var chosen = SampleHelper.ChooseItems(labels);
        CurrentSettingKey = choices[chosen].Key;

        var resolved = CurrentSetting;
        Console.WriteLine($"[调试] 当前模型配置：{CurrentSettingKey} - {resolved.AiPlatform} - {resolved.Endpoint}");
        var chatModel = resolved.ModelName?.Chat;
        if (!chatModel.IsNullOrEmpty())
        {
            Console.WriteLine($"[调试] Chat 模型：{chatModel}");
        }
    }
}
