using Senparc.AI.Kernel;
using Senparc.CO2NET;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.AI.Tests
{
    [TestClass]
    public class RegisterTest : BaseTest
    {
        [TestMethod]
        public void GlobalRegisterTest()
        {
            //local variable test
            var senparcAiSetting = BaseTest._senparcAiSetting; // Senparc.AI.Config.SenparcAiSettings;

            Console.WriteLine(senparcAiSetting.ToJson(true));

            Assert.IsNotNull(senparcAiSetting);
            Assert.AreEqual(AiPlatform.AzureOpenAI, senparcAiSetting.AiPlatform);
            Assert.AreEqual(false, senparcAiSetting.UseNeuCharAI);
            Assert.IsFalse(senparcAiSetting.NeuCharEndpoint.IsNullOrEmpty());
            Assert.IsFalse(senparcAiSetting.ApiKey.IsNullOrEmpty());
            Assert.IsTrue(senparcAiSetting.OrganizationId.IsNullOrEmpty());


            //global variable
            IRegisterService registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(null, new SenparcSetting(true))
                           .UseSenparcGlobal(true, null)
                           .UseSenparcAI();

            Assert.IsNotNull(senparcAiSetting);
            Assert.AreEqual(senparcAiSetting, Senparc.AI.Config.SenparcAiSetting);

            Assert.IsNotNull(senparcAiSetting.AzureOpenAIKeys);
            Assert.IsNotNull(senparcAiSetting.AzureOpenAIKeys.ApiKey);

            Assert.IsNotNull(senparcAiSetting.OpenAIKeys);
            Assert.IsNotNull(senparcAiSetting.OpenAIKeys.ApiKey);

            //test Items multi-model
            var setting = senparcAiSetting as SenparcAiSetting;
            var item = setting.Items["MyNeuCharAI"];
            Assert.IsNotNull(item);
            Assert.AreEqual(AiPlatform.NeuCharAI, item.AiPlatform);
            Assert.AreEqual("MyNeuCharAIKey", item.NeuCharAIKeys.ApiKey);
        }
    }
}
