using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Entities.Tests
{
    [TestClass()]
    public class SenparcAiContextTests
    {
        [TestMethod()]
        public void TryInitExtendContextTest()
        {
            var context = new SenparcAiArguments();
            Assert.IsNotNull(context.KernelArguments);

            //执行初始化
            var extendContextHashCode = context.KernelArguments.GetHashCode();
            Assert.IsNotNull(context.KernelArguments);

            //再次执行初始化
            Assert.AreEqual(extendContextHashCode, context.KernelArguments.GetHashCode());
        }
    }
}