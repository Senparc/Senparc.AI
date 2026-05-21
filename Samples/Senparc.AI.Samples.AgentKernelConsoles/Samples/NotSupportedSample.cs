namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// AgentKernel 暂未实现的能力占位提示。
/// </summary>
public static class NotSupportedSample
{
    public static Task RunAsync(string featureName)
    {
        Console.WriteLine();
        Console.WriteLine($"{featureName}：尚未提供");
        Console.WriteLine("说明：Senparc.AI.AgentKernel 基于 Microsoft Agent Framework，当前示例仅覆盖测试中已验证的 Chat / Completion / Embedding / RAG 能力。");
        Console.WriteLine("如需 Dall·E、Planner、Plugin、STT 等，请使用 Samples/Senparc.AI.Samples.Consoles（Senparc.AI.Kernel）。");
        Console.WriteLine();
        return Task.CompletedTask;
    }
}
