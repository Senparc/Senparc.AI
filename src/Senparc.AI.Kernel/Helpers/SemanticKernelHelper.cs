using Microsoft.SemanticKernel;
using Senparc.AI.Interfaces;

namespace Senparc.AI.Kernel.Helpers;

public class SemanticKernelHelper
{
    public ISenparcAiSetting AiSetting { get; private set; }
    public IKernelBuilder KernelBuilder { get; private set; } = Microsoft.SemanticKernel.Kernel.CreateBuilder();

    public SemanticKernelHelper(ISenparcAiSetting aiSetting)
    {
        AiSetting = aiSetting;
    }

    public void ResetAiSetting(ISenparcAiSetting aiSetting)
    {
        AiSetting = aiSetting;
    }

    public void ResetHttpClient(bool enableLog = false)
    {
    }

    public Microsoft.SemanticKernel.Kernel GetKernel()
    {
        return new Microsoft.SemanticKernel.Kernel();
    }

    public Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string modelName, string text)
    {
        var len = 64;
        var values = new float[len];
        var seed = HashCode.Combine(modelName ?? string.Empty, text ?? string.Empty);
        var rnd = new Random(seed);
        for (var i = 0; i < len; i++)
        {
            values[i] = (float)(rnd.NextDouble() * 2 - 1);
        }
        return Task.FromResult(new ReadOnlyMemory<float>(values));
    }
}
