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
    }
}
