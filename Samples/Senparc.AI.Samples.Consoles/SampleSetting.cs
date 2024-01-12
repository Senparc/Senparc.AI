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

    private void SetModelAsync()
    {

    }

    /// <summary>
    /// 修改 Console 背景颜色
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private void SetBackgroundColorAsync()
    {
        Console.BackgroundColor = SampleHelper.ChooseItems<ConsoleColor>();
    }

    /// <summary>
    /// 修改 Console 字体颜色
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private void SetForegroundColorAsync()
    {
        var color = SampleHelper.ChooseItems<ConsoleColor>();
        //Console.WriteLine("Color:" + color);
        Console.ForegroundColor = color;
    }

    public void RunAsync()
    {
        SettingItems currentChoose = SampleHelper.ChooseItems<SettingItems>();

        var exit = false;
        switch (currentChoose)
        {
            case SettingItems.退出:
                exit = true;
                break;
            case SettingItems.自定义模型:
                 SetModelAsync();
                break;
            case SettingItems.设置背景颜色:
                 SetBackgroundColorAsync();
                break;
            case SettingItems.设置字体颜色:
                 SetForegroundColorAsync();
                break;
            default:
                 Console.Out.WriteLineAsync("请选择正确的选项");
                 RunAsync();
                break;
        }

        if (!exit)
        {
            RunAsync();
        }
    }

}