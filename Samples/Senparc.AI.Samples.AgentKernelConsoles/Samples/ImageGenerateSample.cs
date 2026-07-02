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
            throw new InvalidOperationException("This sample requires AgentAiHandler.");
        }

        var setting = ((SenparcAiSetting)SampleSetting.CurrentSetting);//.Items["AzureGptImage2"];

        Console.WriteLine("ImageGenerate Sample: Configuring TextToImage model and building kernel...");

        var iWantToRun = agentHandler.IWantTo(setting)
            .ConfigImageModel("Jeffrey")
            .BuildKernel();

        var kernel = iWantToRun.Kernel;

        Console.WriteLine("ConfigModels: " + string.Join(",", kernel.ConfigModels.Select(z => z.ToString())));
        Console.WriteLine("ImageClient: " + (kernel.ImageClient?.ToString() ?? "null"));

        Console.WriteLine("Enter the image description to generate, or enter exit to leave:");

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
                Console.WriteLine($"Generation completed in {SystemTime.NowDiff(dt1).TotalSeconds}s");
                var imageBytes = image.Value.ImageBytes;

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    var file = $"Senparc.AI.Image-{SystemTime.NowTicks}.png";
                    await File.WriteAllBytesAsync(file, imageBytes);
                    Console.WriteLine("Image saved: " + file);

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
                        Console.WriteLine("Failed to open image: " + ex.Message);
                    }

                    lastImage = file;
                }
                else
                {
                    Console.WriteLine("No image bytes were returned.\nEnter another description to generate the next image, or enter exit to leave.");
                }

                Console.WriteLine("Enter another description to generate the next image, or enter exit to leave.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Image generation failed: " + ex.Message);
            }
        }
    }
}
