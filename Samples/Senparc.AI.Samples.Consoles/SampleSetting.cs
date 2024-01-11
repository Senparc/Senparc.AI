using Senparc.AI.Samples.Consoles;

public class SampleSetting
{
    private enum SettingItems
    {
        退出 = 0,
        自定义模型 = 1,
        设置背景颜色 = 2,
        设置字体颜色 = 3,
    }

    private async Task SetModelAsync()
    {

    }

    /// <summary>
    /// 修改 Console 背景颜色
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task SetBackgroundColorAsync()
    {
        Console.BackgroundColor = SampleHelper.ChooseItems<ConsoleColor>();
    }

    /// <summary>
    /// 修改 Console 字体颜色
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task SetForegroundColorAsync()
    {
        Console.ForegroundColor = SampleHelper.ChooseItems<ConsoleColor>();
    }
    public async Task RunAsync()
    {
        SettingItems currentChoose = SampleHelper.ChooseItems<SettingItems>();


    Start:
        var exit = false;
        switch (currentChoose)
        {
            case SettingItems.退出:
                exit = true;
                break;
            case SettingItems.自定义模型:
                await SetModelAsync();
                break;
            case SettingItems.设置背景颜色:
                await SetBackgroundColorAsync();
                break;
            case SettingItems.设置字体颜色:
                await SetForegroundColorAsync();
                break;
            default:
                await Console.Out.WriteLineAsync("请选择正确的选项");
                goto Start;
        }

        if (!exit)
        {
            goto Start;
        }
    }

}