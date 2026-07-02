namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Placeholder prompt for AgentKernel capabilities that are not implemented yet.
/// </summary>
public static class NotSupportedSample
{
    public static Task RunAsync(string featureName)
    {
        Console.WriteLine();
        Console.WriteLine($"{featureName}:Not available yet");
        Console.WriteLine("Senparc.AI.AgentKernel is based on Microsoft Agent Framework. This sample currently covers Chat, Completion, Embedding, RAG, Image, STT, and TTS.");
        Console.WriteLine("Use Samples/Senparc.AI.Samples.Consoles (Senparc.AI.Kernel) for Planner, Plugin, and related capabilities.");
        Console.WriteLine();
        return Task.CompletedTask;
    }
}
