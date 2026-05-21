namespace Senparc.AI.Samples.AgentKernelConsoles;

public static class SampleHelper
{
    public static string GetAppSettingsFile()
    {
        if (File.Exists("appsettings.Development.json"))
        {
            Console.WriteLine("[调试] 使用 appsettings.Development.json");
            return "appsettings.Development.json";
        }

        Console.WriteLine("[调试] 使用 appsettings.json");
        return "appsettings.json";
    }

    public static T ChooseItems<T>() where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        for (var i = 0; i < values.Length; i++)
        {
            Console.WriteLine($"[{i}] {values[i]}");
        }

        var input = Console.ReadLine();
        if (!int.TryParse(input, out var index) || index < 0 || index >= values.Length)
        {
            Console.WriteLine("序号无效，已选择第一项。");
            return values[0];
        }

        return values[index];
    }

    public static int ChooseItems(string[] items)
    {
        for (var i = 0; i < items.Length; i++)
        {
            Console.WriteLine($"[{i}] {items[i]}");
        }

        var input = Console.ReadLine();
        if (!int.TryParse(input, out var index) || index < 0 || index >= items.Length)
        {
            Console.WriteLine("序号无效，已选择第一项。");
            return 0;
        }

        return index;
    }

    public static void PrintNote(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }
}
