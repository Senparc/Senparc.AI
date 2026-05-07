using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Entities.Tests
{
    [TestClass()]
    public class PromptConfigParameterTests
    {
        public class TestConfigClass
        {
            public int MaxTokens { get; set; }
            public double Temperature { get; set; }
            public double TopP { get; set; }
        }

        [TestMethod()]
        public void TrySetTest()
        {
            //var promptConfigParameter = new PromptConfigParameter();
            //promptConfigParameter.MaxTokens = 1000;
            //promptConfigParameter.Temperature = 1;

            //TestConfigClass testConfigClass = new TestConfigClass()
            //{
            //    TopP = 10,
            //};

            //promptConfigParameter.TrySet(c => c.MaxTokens, testConfigClass, () => testConfigClass.MaxTokens);
            //promptConfigParameter.TrySet(c => c.Temperature, testConfigClass, () => testConfigClass.Temperature);
            //promptConfigParameter.TrySet(c => c.TopP, testConfigClass, () => testConfigClass.TopP
            //);


            //Assert.AreEqual(promptConfigParameter.MaxTokens, testConfigClass.MaxTokens);
            //Assert.AreEqual(testConfigClass.Temperature, promptConfigParameter.Temperature);
            //Assert.AreEqual(10, promptConfigParameter.TopP);//未设置
        }
    }
}