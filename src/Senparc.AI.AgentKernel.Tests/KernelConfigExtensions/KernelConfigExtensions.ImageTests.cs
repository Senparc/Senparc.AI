using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using System.Linq;

namespace Senparc.AI.AgentKernel.Tests.KernelConfigExtensions
{
    [TestClass]
    public class KernelConfigExtensionsImageTests : KernelTestBase
    {
        [TestMethod]
        public async Task ConfigImageGenerationTest()
        {
            var handler = new AgentAiHandler(_senparcAiSetting);

            var iWantToRun = handler.IWantTo()
                .ConfigImageModel("Jeffrey")
                .BuildKernel();

            var kernel = iWantToRun.Kernel;

            Assert.IsNotNull(kernel);
            Assert.IsTrue(kernel.ConfigModels.Contains(Senparc.AI.ConfigModel.TextToImage));
            Assert.IsNotNull(kernel.ImageClient);

            //var result = await iWantToRun.Kernel.ImageClient.("Generate an image of a cute cat playing on grass.");
        }
    }
}
