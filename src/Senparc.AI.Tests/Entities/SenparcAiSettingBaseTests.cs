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

            //不进行任何设置
            Assert.IsFalse(senparcAiSetting.IsOpenAiKeysSetted);

            //设置 ApiKey
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

        [TestMethod]
        public void A2AAgents_Default_ShouldNotBeNull()
        {
            var senparcAiSetting = new SenparcAiSettingBase();
            Assert.IsNotNull(senparcAiSetting.A2AAgents);
            Assert.AreEqual(0, senparcAiSetting.A2AAgents.Count);
        }
    }
}
