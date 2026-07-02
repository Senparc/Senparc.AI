using Microsoft.SemanticKernel.TextToImage;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public class DallESample
    {
        IAiHandler _aiHandler;
        IServiceProvider _serviceProvider;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public DallESample(IAiHandler aiHandler, IServiceProvider serviceProvider)
        {
            _aiHandler = aiHandler;
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//Synchronize logging setting state
            _serviceProvider = serviceProvider;
        }

        public async Task RunAsync()
        {
            var dalleSetting = ((SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting)["AzureDallE3"];
            if ((
                    dalleSetting.OpenAIKeys == null ||
                    dalleSetting.OpenAIKeys.ApiKey.IsNullOrEmpty() ||
                    dalleSetting.OpenAIKeys.OrganizationId.IsNullOrEmpty()
                ) &&
                (
                    dalleSetting.AzureOpenAIKeys == null ||
                    dalleSetting.AzureOpenAIKeys.ApiKey.IsNullOrEmpty()
                ) &&
                (
                    dalleSetting.AiPlatform != AiPlatform.OpenAI && dalleSetting.AiPlatform != AiPlatform.AzureOpenAI
                )
               )
            {
                await Console.Out.WriteLineAsync("The DALL-E API requires an OpenAI or Azure OpenAI ApiKey before use.");
                return;
            }

            await Console.Out.WriteLineAsync("DALL-E 3 started. Enter the image generation prompt, enter exit to leave, or enter s to save the previous generated image.");

            var userId = "Jeffrey";
            var iWantTo = _semanticAiHandler.IWantTo(dalleSetting)
                                .ConfigModel(ConfigModel.TextToImage, userId)
                                .BuildKernel();

            var dallE = iWantTo.GetRequiredService<ITextToImageService>();

            string request;
            string lastImageUrl = null;
            while ((request = Console.ReadLine()) != "exit")
            {
                Console.WriteLine();
                if (request.Equals("S", StringComparison.OrdinalIgnoreCase))
                {
                    //Save image
                    if (lastImageUrl.IsNullOrEmpty())
                    {
                        await Console.Out.WriteLineAsync("No successfully generated image is available.");
                    }

                    var filePath = Senparc.CO2NET.Utilities.ServerUtility.ContentRootMapPath($"~/Senparc.AI.Dalle-{SystemTime.NowTicks}.jpg");

                    using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        await Senparc.CO2NET.HttpUtility.Get.DownloadAsync(_serviceProvider, lastImageUrl, fs);
                        await fs.FlushAsync();
                        await Console.Out.WriteLineAsync("Image saved:" + filePath);
                    }
                }
                else
                {
                    await Console.Out.WriteLineAsync("Generating, please wait...");

                    //Generate image
                    var imageDescription = request;// "A car fly in the sky, with a panda driver.";
                    lastImageUrl = await dallE.GenerateImageAsync(imageDescription, 1024, 1024);

                    await Console.Out.WriteLineAsync("Generation succeeded. Image URL:" + lastImageUrl);

                    //return:
                    //Image URL:https://oaidalleapiprodscus.blob.core.windows.net/private/org-Bp9B5eGmPFtwDsnIwCV7UjKO/user-1v2aYDuCvJZl0m94gVtYOloH/img-NvtqM7hTcevNKhjpYjVA3Bwl.png?st=2023-04-09T06%3A36%3A55Z&se=2023-04-09T08%3A36%3A55Z&sp=r&sv=2021-08-06&sr=b&rscd=inline&rsct=image/png&skoid=6aaadede-4fb3-4698-a8f6-684d7786b067&sktid=a48cca56-e6da-484e-a814-9c849652bcb3&skt=2023-04-08T19%3A47%3A37Z&ske=2023-04-09T19%3A47%3A37Z&sks=b&skv=2021-08-06&sig=YPSGt9EvAACViGuoDPWoJ/8rnfVy9xu512/blVEYOl0%3D
                }

                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("Please continue entering input:");

            }


        }
    }
}
