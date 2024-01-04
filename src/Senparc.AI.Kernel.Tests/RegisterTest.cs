using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel.Tests.BaseSupport;
using Senparc.AI.Tests;

namespace Senparc.AI.Kernel.Tests
{
    [TestClass]
    public class RegisterTest : KernelTestBase
    {
        [TestMethod]
        public void KernelRegisterTest()
        {
            var settings = Senparc.AI.Config.SenparcAiSetting;
            Assert.IsNotNull(settings);
            Assert.IsInstanceOfType(settings, typeof(SenparcAiSetting));
            Assert.AreEqual(settings,Senparc.AI.Kernel.Config.SenparcAiSetting);

            //TODO: ²âÊÔ NeuChar ½Ó¿Ú
            Assert.AreEqual(AiPlatform.AzureOpenAI, settings.AiPlatform);
            Assert.AreEqual(true, settings.UseAzureOpenAI);
            Assert.AreEqual(false, settings.UseNeuCharAI);
        }

        [TestMethod]
        public void HuggingFaceSettingTest()
        {
            var settings = Senparc.AI.Config.SenparcAiSetting;
            Assert.AreEqual("https://HuggingfaceAPI", settings.HuggingFaceEndpoint);
        }
    }
}