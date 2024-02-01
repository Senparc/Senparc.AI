using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Helpers;
using Senparc.AI.Kernel.Tests.BaseSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.AI.Kernel.Handlers;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Kernel.Helpers.Tests
{


    [TestClass]
    public class SemanticKernelHelperTests : KernelTestBase
    {
        [TestMethod]
        public void GetServiceIdTest()
        {
            var helper = new SemanticKernelHelper();
            var modelName = "MyModelName";
            var result = helper.GetServiceId("Jeffrey", modelName);
            Assert.AreEqual($"Jeffrey-{modelName}", result);
        }

        [TestMethod]
        public void GetKernelTest()
        {
            var helper = new SemanticKernelHelper();

            //测试第一次获取
            var kernel = helper.GetKernel();
            Assert.IsNotNull(kernel);

            //第二次获取，为同一个对象
            var kernel2 = helper.GetKernel();
            Assert.AreEqual(kernel, kernel2);


            //测试刷新
            var kernel3 = helper.GetKernel(refresh: true);
            Assert.AreNotEqual(kernel, kernel3);


            //测试添加配置
            bool testPass = false;
            Action<IKernelBuilder> kernelBuilderAction = kb =>
            {
                testPass = true;
            };
            var kernel4 = helper.GetKernel(kernelBuilderAction: kernelBuilderAction, refresh: true);
            Assert.IsNotNull(kernel4);
            Assert.AreNotEqual(kernel3, kernel4);
            Assert.IsTrue(testPass);
        }

        [TestMethod]
        public void ConfigTest()
        {
            ISenparcAiSetting senparcAiSetting = Senparc.AI.Config.SenparcAiSetting;
            var helper = new SemanticKernelHelper();
            var kernel = helper.ConfigTextCompletion("Jeffrey", senparcAiSetting: senparcAiSetting);
            Assert.IsNotNull(kernel);
        }


        [TestMethod()]
        public async Task ConfigImageGenerationTest()
        {
            //return;

            var dalleSetting = ((SenparcAiSetting)Senparc.AI.Config.SenparcAiSetting)["AzureDallE3"];

            await Console.Out.WriteLineAsync(dalleSetting.ToJson(true));

            var handler = new SemanticAiHandler(dalleSetting);
            var userId = "Jeffrey";
            var iWantTo = handler.IWantTo(dalleSetting)
                                .ConfigModel(ConfigModel.ImageGeneration, userId)
                                .BuildKernel();

#pragma warning disable SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            var dallE = iWantTo.GetRequiredService<ITextToImageService>();
            var imageDescription = "A car fly in the sky, with a panda driver.";
            var image = await dallE.GenerateImageAsync(imageDescription, 1024, 1024);
#pragma warning restore SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            await Console.Out.WriteLineAsync("Image URL:" + image);
            //返回：
            //Image URL:https://oaidalleapiprodscus.blob.core.windows.net/private/org-Bp9B5eGmPFtwDsnIwCV7UjKO/user-1v2aYDuCvJZl0m94gVtYOloH/img-NvtqM7hTcevNKhjpYjVA3Bwl.png?st=2023-04-09T06%3A36%3A55Z&se=2023-04-09T08%3A36%3A55Z&sp=r&sv=2021-08-06&sr=b&rscd=inline&rsct=image/png&skoid=6aaadede-4fb3-4698-a8f6-684d7786b067&sktid=a48cca56-e6da-484e-a814-9c849652bcb3&skt=2023-04-08T19%3A47%3A37Z&ske=2023-04-09T19%3A47%3A37Z&sks=b&skv=2021-08-06&sig=YPSGt9EvAACViGuoDPWoJ/8rnfVy9xu512/blVEYOl0%3D


        }

    }

}
