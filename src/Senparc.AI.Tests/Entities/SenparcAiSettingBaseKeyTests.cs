using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Tests.Entities
{
    [TestClass]
    public class SenparcAiSettingBaseKeyTests:BaseTest
    {
        [TestMethod]
        public void ModelNameTest()
        {
            Console.WriteLine(Senparc.AI.Config.SenparcAiSetting.ToJson(true));
            var setting = Senparc.AI.Config.SenparcAiSetting as SenparcAiSetting;

            Assert.AreEqual(AiPlatform.AzureOpenAI, setting.AiPlatform);
            Assert.AreEqual("https://www.neuchar.com/2", setting.NeuCharAIKeys.NeuCharEndpoint);

            Assert.AreEqual("gpt-35-turbo", setting.NeuCharAIKeys.ModelName.Chat);
            Assert.AreEqual("gpt-35-turbo-instruct", setting.NeuCharAIKeys.ModelName.TextCompletion);
            Assert.AreEqual("gpt-4o", setting.AzureOpenAIKeys.ModelName.Chat);
            Assert.AreEqual("gpt-35-turbo-instruct", setting.AzureOpenAIKeys.ModelName.TextCompletion);
            Assert.AreEqual("dall-e-3", setting["AzureDallE3"].AzureOpenAIKeys.ModelName.TextToImage);
        }
    }
}
