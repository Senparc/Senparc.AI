using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Tests.Entities
{
    [TestClass]
    public class SenparcAiSettingBaseTests : BaseTest
    {
        [TestMethod]
        public void OpenAiKeysSettedTest()
        {
            var senparcAiSetting = new SenparcAiSettingBase();

            //Do not apply any settings
            Assert.IsFalse(senparcAiSetting.IsOpenAiKeysSetted);

            //Set ApiKey
            senparcAiSetting.OpenAIKeys = new OpenAIKeys()
            {
                ApiKey = "MyApiKey"
            };
            Assert.IsTrue(senparcAiSetting.IsOpenAiKeysSetted);

        }

        [TestMethod]
        public void McpServers_Default_ShouldNotBeNull()
        {
            var senparcAiSetting = new SenparcAiSettingBase();
            Assert.IsNotNull(senparcAiSetting.McpServers);
            Assert.AreEqual(0, senparcAiSetting.McpServers.Count);
        }
    }
}
