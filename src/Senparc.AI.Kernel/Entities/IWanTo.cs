using System.Collections.Concurrent;
using Microsoft.SemanticKernel;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Helpers;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.TextToImage;

namespace Senparc.AI.Kernel.Handlers;

public class IWantTo
{
    public ConcurrentDictionary<string, object> TempStore { get; set; } = new();
    public IKernelBuilder KernelBuilder { get; set; } = Microsoft.SemanticKernel.Kernel.CreateBuilder();
    public SemanticAiHandler SemanticAiHandler { get; set; }
    public SemanticKernelHelper SemanticKernelHelper => SemanticAiHandler.SemanticKernelHelper;
    public ISenparcAiSetting SenparcAiSetting { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;

    public IWantTo(SemanticAiHandler handler, ISenparcAiSetting? senparcAiSetting)
    {
        SemanticAiHandler = handler;
        SenparcAiSetting = senparcAiSetting ?? handler.SenparcAiSetting;
    }
}

public class IWantToConfig
{
    public IWantTo IWantTo { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;

    public IWantToConfig(IWantTo iWantTo)
    {
        IWantTo = iWantTo;
    }
}

public class IWantToBuild
{
    public IWantToConfig IWantToConfig { get; set; }

    public IWantToBuild(IWantToConfig iWantToConfig)
    {
        IWantToConfig = iWantToConfig;
    }
}

public class IWantToRun
{
    public IWantToBuild IWantToBuild { get; set; }
    public SenparcAiArguments StoredAiArguments { get; set; } = new();
    public PromptConfigParameter? PromptConfigParameter { get; set; }
    public List<KernelFunction> Functions { get; set; } = [];
    public SemanticKernelHelper SemanticKernelHelper => IWantToBuild.IWantToConfig.IWantTo.SemanticKernelHelper;
    public Microsoft.SemanticKernel.Kernel Kernel => SemanticKernelHelper.GetKernel();

    public IWantToRun(IWantToBuild iWantToBuild)
    {
        IWantToBuild = iWantToBuild;
    }

    public T GetRequiredService<T>(string? name = null) where T : class
    {
        if (typeof(T) == typeof(ITextToImageService))
        {
            return (T)(object)new CompatTextToImageService();
        }

        if (typeof(T) == typeof(IAudioToTextService))
        {
            return (T)(object)new CompatAudioToTextService();
        }

        throw new NotSupportedException($"Compat service not supported: {typeof(T).FullName}");
    }
}
