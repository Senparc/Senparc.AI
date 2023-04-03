using Microsoft.Extensions.DependencyInjection;
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

            Assert.AreEqual(AiPlatform.AzureOpenAI, settings.AiPlatform);
            Assert.AreEqual(true, settings.UseAzureOpenAI);
        }
    }
}