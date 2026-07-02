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

            //ExecuteInitialize
            var extendContextHashCode = context.KernelArguments.GetHashCode();
            Assert.IsNotNull(context.KernelArguments);

            //Execute initialization again
            Assert.AreEqual(extendContextHashCode, context.KernelArguments.GetHashCode());
        }
    }
}