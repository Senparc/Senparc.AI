using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Tests.BaseSupport;

namespace Senparc.AI.AgentKernel.Tests
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
            Assert.AreEqual(settings, Senparc.AI.AgentKernel.Config.SenparcAiSetting);

            //TODO: test NeuChar API
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

        [TestMethod]
        public void ItemsTest()
        {
            var settings = Senparc.AI.Config.SenparcAiSetting as SenparcAiSetting;

            // fix CS8602: ensure settings not null
            Assert.IsNotNull(settings);

            Assert.AreEqual(3, settings.Items.Count);

            var dalle3Setting = settings.Items["AzureDallE3"];

            Assert.IsNotNull(dalle3Setting);
            Assert.AreEqual(settings["AzureDallE3"], dalle3Setting);//indexer retrieval passed
            Assert.AreEqual("2022-12-01", dalle3Setting.AzureOpenAIApiVersion);

            var neucharAISetting = settings["MyNeuCharAI"].NeuCharAIKeys;
            Assert.IsNotNull(neucharAISetting);
            Assert.AreEqual("MyNeuCharAIKey", neucharAISetting.ApiKey);
            Assert.AreEqual("https://www.neuchar.com/2/", neucharAISetting.NeuCharEndpoint);
        }
    }
}