using Microsoft.SemanticKernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel.Helpers;
using Senparc.AI.Kernel.Tests.BaseSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Tests.Helpers
{
    [TestClass]
    public class SemanticKernelHelperTests : KernelTestBase
    {
        [TestMethod]
        public void GetServiceIdTest()
        {
            var helper = new SemanticKernelHelper();
            var result = helper.GetServiceId("Jeffrey", "text-davinci-003");
            Assert.AreEqual("Jeffrey-text-davinci-003", result);
        }

        [TestMethod]
        public void GetKernelTest()
        {
            var helper = new SemanticKernelHelper();

            //测试第一次获取
            var kernel = helper.GetKernel();
            Assert.IsNotNull(kernel);

            //第二次获取，为同一个对象
            var kernel2 = helper.GetKernel();
            Assert.AreEqual(kernel, kernel2);


            //测试刷新
            var kernel3 = helper.GetKernel(refresh: true);
            Assert.AreNotEqual(kernel, kernel3);


            //测试添加配置
            bool testPass = false;
            Action<KernelBuilder> kernelBuilderAction = kb =>
            {
                testPass = true;
            };
            var kernel4 = helper.GetKernel(kernelBuilderAction: kernelBuilderAction, refresh: true);
            Assert.IsNotNull(kernel4);
            Assert.AreNotEqual(kernel3, kernel4);
            Assert.IsTrue(testPass);
        }

        [TestMethod]
        public void ConfigTest()
        {
            var helper = new SemanticKernelHelper();
            var kernel = helper.ConfigTextCompletion("Jeffrey", "text-davinci-003",null);
            Assert.IsNotNull(kernel);
        }

    }

}
