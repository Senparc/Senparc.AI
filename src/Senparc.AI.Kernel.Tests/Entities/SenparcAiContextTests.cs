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
            Assert.IsNull(context.ExtendContext);

            //执行初始化
            context.TryInitExtendContext();
            var extendContextHashCode = context.ExtendContext.GetHashCode();
                        Assert.IsNotNull(context.ExtendContext);

            //再次执行初始化
            context.TryInitExtendContext();
            Assert.AreEqual(extendContextHashCode, context.ExtendContext.GetHashCode());
        }
    }
}