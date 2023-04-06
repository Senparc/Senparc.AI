using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Tests.BaseSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Handlers.Tests
{
    [TestClass()]
    public class KernelConfigExtensionTests:KernelTestBase
    {
        [TestMethod()]
        public void ConfigModelTest()
        {
            var handler = new SemanticAiHandler();
            var userId = "JeffreySu";

            //测试 TextEmbedding
            handler
                .IWantTo()
                .ConfigModel(ConfigModel.TextEmbedding, userId, "text-embedding-ada-002")
                .
            //handler



        }
    }
}