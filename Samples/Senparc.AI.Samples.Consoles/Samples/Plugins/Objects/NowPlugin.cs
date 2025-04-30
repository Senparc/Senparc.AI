using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;

namespace DefaultNamespace;

[Description("时间插件")]
public class NowPlugin
{
    [KernelFunction("GetCurrentNowTime"), Description("获取当前时间")]
    public static async Task<string> GetCurrentTime([Description("C# 时间格式")] string format)
    {
        Console.WriteLine("[运行 GetCurrentTime] function-calling]");
        return  DateTime.Now.ToString(format);
    }


    [KernelFunction("AddTime"), Description("修改时间")]
    public static DateTime AddTime([Description("需要修改的时间，C# 的时间格式")] DateTime? dateTime, int days, double seconds)
    {
        Console.WriteLine($"[运行 AddTime] function-calling {dateTime},{days},{seconds}]");
        return (dateTime??DateTime.Now).AddSeconds(seconds).AddDays(days);
    }
}