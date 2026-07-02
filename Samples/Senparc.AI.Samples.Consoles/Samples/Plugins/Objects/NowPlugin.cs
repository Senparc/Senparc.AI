using Microsoft.SemanticKernel;
using Senparc.AI.Samples.Consoles;
using System;
using System.ComponentModel;

namespace DefaultNamespace;

[Description("Time plugin")]
public class NowPlugin
{
    [KernelFunction("GetCurrentNowTime"), Description("Get current time")]
    public static async Task<string> GetCurrentTime([Description("C# time format")] string format)
    {
        SampleHelper.PrintNote("[Run GetCurrentTime] function-calling");
        Console.WriteLine();
        return DateTime.Now.ToString(format);
    }


    [KernelFunction("AddTime"), Description("Modify time")]
    public static DateTime AddTime([Description("Time to modify, in C# time format")] DateTime? dateTime, int days, double seconds)
    {
        SampleHelper.PrintNote("[Run AddTime] function-calling");
        Console.WriteLine();
        return (dateTime ?? DateTime.Now).AddSeconds(seconds).AddDays(days);
    }
}