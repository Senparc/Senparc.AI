using Microsoft.SemanticKernel.AI.ImageGeneration;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Helpers;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _serviceProvider = serviceProvider;
        }


        public async Task RunAsync()
        {
            if (Senparc.AI.Config.SenparcAiSetting.OpenAIKeys == null ||
                Senparc.AI.Config.SenparcAiSetting.OpenAIKeys.ApiKey.IsNullOrEmpty() ||
                Senparc.AI.Config.SenparcAiSetting.OpenAIKeys.OrgaizationId.IsNullOrEmpty()
                )
            {
                await Console.Out.WriteLineAsync("DallE 接口需要设置 OpenAI ApiKey 后才能使用！");
                return;
            }

            await Console.Out.WriteLineAsync("DallE 开始运行，请输入需要生成图片的内容，输入 exit 退出，输入s 保存上一张生成的图片。");

            var userId = "Jeffrey";
            var iWantTo = _semanticAiHandler.IWantTo()
                                .ConfigModel(ConfigModel.ImageGeneration, userId, "image-generation")
                                .BuildKernel();

            var dallE = iWantTo.GetService<IImageGeneration>();

            string request;
            string lastImageUrl = null;
            while ((request = Console.ReadLine()) != "exit")
            {
                Console.WriteLine();
                if (request.Equals("S", StringComparison.OrdinalIgnoreCase))
                {
                    //保存图片
                    if (lastImageUrl.IsNullOrEmpty())
                    {
                        await Console.Out.WriteLineAsync("尚无成功生成的图片！");
                    }

                    var filePath = Senparc.CO2NET.Utilities.ServerUtility.ContentRootMapPath($"~/Senparc.AI.Dalle-{SystemTime.NowTicks}.jpg");

                    using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        await Senparc.CO2NET.HttpUtility.Get.DownloadAsync(_serviceProvider, lastImageUrl, fs);
                        await fs.FlushAsync();
                        await Console.Out.WriteLineAsync("图片已保存：" + filePath);
                    }
                }
                else
                {
                    //生成图片
                    var imageDescription = request;// "A car fly in the sky, with a panda driver.";
                    lastImageUrl = await dallE.GenerateImageAsync(imageDescription, 256, 256);

                    await Console.Out.WriteLineAsync("生成成功！Image URL:" + lastImageUrl);

                    //返回：
                    //Image URL:https://oaidalleapiprodscus.blob.core.windows.net/private/org-Bp9B5eGmPFtwDsnIwCV7UjKO/user-1v2aYDuCvJZl0m94gVtYOloH/img-NvtqM7hTcevNKhjpYjVA3Bwl.png?st=2023-04-09T06%3A36%3A55Z&se=2023-04-09T08%3A36%3A55Z&sp=r&sv=2021-08-06&sr=b&rscd=inline&rsct=image/png&skoid=6aaadede-4fb3-4698-a8f6-684d7786b067&sktid=a48cca56-e6da-484e-a814-9c849652bcb3&skt=2023-04-08T19%3A47%3A37Z&ske=2023-04-09T19%3A47%3A37Z&sks=b&skv=2021-08-06&sig=YPSGt9EvAACViGuoDPWoJ/8rnfVy9xu512/blVEYOl0%3D
                }


                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("请继续输入：");

            }


        }
    }
}
