using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;

namespace DefaultNamespace;

[Description("时间插件")]
public class NowPlugin
{
    [KernelFunction("GetCurrentNowTime"), Description("获取当前时间")]
    public static async Task<string> GetCurrentTime([Description("C# 时间格式")]string format) => DateTime.Now.ToString(format);
}
