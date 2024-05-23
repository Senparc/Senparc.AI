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

            //TODO: 测试 NeuChar 接口
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
            Assert.AreEqual(3, settings.Items.Count);

            var dalle3Setting = settings.Items["AzureDallE3"];

            Assert.IsNotNull(dalle3Setting);
            Assert.AreEqual(settings["AzureDallE3"], dalle3Setting);//两种获取方式通用
            Assert.AreEqual("2022-12-01", dalle3Setting.AzureOpenAIApiVersion);

            var neucharAISetting = settings["MyNeuCharAI"].NeuCharAIKeys;
            Assert.IsNotNull(neucharAISetting);
            Assert.AreEqual("MyNeuCharAIKey", neucharAISetting.ApiKey);
            Assert.AreEqual("https://www.neuchar.com/2/", neucharAISetting.NeuCharEndpoint);
        }
    }
}