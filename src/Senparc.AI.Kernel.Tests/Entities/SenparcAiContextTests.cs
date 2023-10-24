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
            var context = new SenparcAiContext();
            Assert.IsNotNull(context.ContextVariables);

            //执行初始化
            var extendContextHashCode = context.ContextVariables.GetHashCode();
            Assert.IsNotNull(context.ContextVariables);

            //再次执行初始化
            Assert.AreEqual(extendContextHashCode, context.ContextVariables.GetHashCode());
        }
    }
}