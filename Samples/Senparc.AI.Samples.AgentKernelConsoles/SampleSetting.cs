using Senparc.AI.AgentKernel;
using Senparc.AI.Interfaces;

namespace Senparc.AI.Samples.AgentKernelConsoles;

public class SampleSetting
{
    internal static string CurrentSettingKey { get; set; } = "Default";
    internal static bool EnableHttpClientLog { get; set; }

    internal static ISenparcAiSetting CurrentSetting =>
        CurrentSettingKey == "Default"
            ? Senparc.AI.Config.SenparcAiSetting
            : ((SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting)[CurrentSettingKey];

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
        var settings = new Dictionary<string, ISenparcAiSetting>
        {
            ["Default"] = Senparc.AI.Config.SenparcAiSetting
        };

        if (Senparc.AI.Config.SenparcAiSetting is SenparcAiSetting aiSetting)
        {
            foreach (var item in aiSetting.Items ?? [])
            {
                settings[item.Key] = item.Value;
            }
        }

        var keys = settings.Keys.ToArray();
        var chosen = SampleHelper.ChooseItems(keys);
        CurrentSettingKey = keys[chosen];
        Console.WriteLine($"[调试] 当前模型配置：{CurrentSettingKey} - {CurrentSetting.AiPlatform}");
    }
}
