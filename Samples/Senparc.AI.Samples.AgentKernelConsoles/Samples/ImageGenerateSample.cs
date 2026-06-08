using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using System.Linq;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

public class ImageGenerateSample
{
    private readonly IAiHandler _aiHandler;

    public ImageGenerateSample(IAiHandler aiHandler)
    {
        _aiHandler = aiHandler;
    }

    public async Task RunAsync()
    {
        if (_aiHandler is not AgentAiHandler agentHandler)
        {
            throw new InvalidOperationException("当前示例需要 AgentAiHandler。");
        }

        var setting = SampleSetting.CurrentSetting;

        Console.WriteLine("ImageGenerate Sample: Configuring TextToImage model and building kernel...");

        var iWantToRun = agentHandler.IWantTo(setting)
            .ConfigImageModel("Jeffrey")
            .BuildKernel();

        var kernel = iWantToRun.Kernel;

        Console.WriteLine("ConfigModels: " + string.Join(",", kernel.ConfigModels.Select(z => z.ToString())));
        Console.WriteLine("ImageClient: " + (kernel.ImageClient?.ToString() ?? "null"));

        Console.WriteLine("示例已完成（此 Demo 仅演示配置与客户端注入）；真正的图生图生成请在 Samples.Consoles 项目中的 DallE 示例中使用 ITextToImageService。");
    }
}
