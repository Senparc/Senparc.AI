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
            //本地变量测试
            var senparcAiSetting = BaseTest._senparcAiSetting; // Senparc.AI.Config.SenparcAiSettings;
            Assert.IsNotNull(senparcAiSetting);
            Assert.AreEqual(AiPlatform.NeuCharOpenAI, senparcAiSetting.AiPlatform);
            Assert.AreEqual(true, senparcAiSetting.UseNeuCharOpenAI);
            Assert.IsFalse(senparcAiSetting.NeuCharEndpoint.IsNullOrEmpty());
            Assert.IsFalse(senparcAiSetting.ApiKey.IsNullOrEmpty());
            Assert.IsFalse(senparcAiSetting.OrgaizationId.IsNullOrEmpty());


            //全局变量
            IRegisterService registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(null, new SenparcSetting(true));
            registerService.UseSenparcGlobal(true, null)
                           .UseSenparcAI(senparcAiSetting);

            Assert.IsNotNull(senparcAiSetting);
            Assert.AreEqual(senparcAiSetting, Senparc.AI.Config.SenparcAiSetting);

            Assert.IsNotNull(senparcAiSetting.AzureOpenAIKeys);
            Assert.IsNotNull(senparcAiSetting.AzureOpenAIKeys.ApiKey);

            Assert.IsNotNull(senparcAiSetting.OpenAIKeys);
            Assert.IsNotNull(senparcAiSetting.OpenAIKeys.ApiKey);
        }
    }
}
