using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using System.Linq;
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Azure.AI.OpenAI.Images;

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

        var setting = ((SenparcAiSetting)SampleSetting.CurrentSetting);//.Items["AzureGptImage2"];

        Console.WriteLine("ImageGenerate Sample: Configuring TextToImage model and building kernel...");

        var iWantToRun = agentHandler.IWantTo(setting)
            .ConfigImageModel("Jeffrey")
            .BuildKernel();

        var kernel = iWantToRun.Kernel;

        Console.WriteLine("ConfigModels: " + string.Join(",", kernel.ConfigModels.Select(z => z.ToString())));
        Console.WriteLine("ImageClient: " + (kernel.ImageClient?.ToString() ?? "null"));

        Console.WriteLine("请输入需要生成的图像描述（输入 exit 退出）：");

        string lastImage = null;
        while (true)
        {
            var input = Console.ReadLine();
            if (input.IsNullOrEmpty()) continue;
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

            Console.WriteLine("Generating...");
            var dt1 = SystemTime.Now;
            try
            {
                var image = await kernel.ImageGenerationAsync(input, 1024, 1024);

                if (image.Value.ImageUri != null)
                {
                    lastImage = image.Value.ImageUri.ToString();
                }
                Console.WriteLine($"生成完成，耗时：{SystemTime.NowDiff(dt1).TotalSeconds}s");
                var imageBytes = image.Value.ImageBytes;

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    var file = $"Senparc.AI.Image-{SystemTime.NowTicks}.png";
                    await File.WriteAllBytesAsync(file, imageBytes);
                    Console.WriteLine("图片已保存：" + file);

                    try
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            Process.Start("open", file);
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            Process.Start("xdg-open", file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("打开图片失败：" + ex.Message);
                    }

                    lastImage = file;
                }
                else
                {
                    Console.WriteLine("未返回图片字节。\n继续输入描述生成下一张图片。输入 exit 结束。");
                }

                Console.WriteLine("继续输入描述生成下一张图片。输入 exit 结束。");
            }
            catch (Exception ex)
            {
                Console.WriteLine("生成图片失败：" + ex.Message);
            }
        }
    }
}
