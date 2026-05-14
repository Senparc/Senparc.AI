using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.AgentKernel.Tests.Kernels
{
    [TestClass]
    public class AiKernelTest : KernelTestBase
    {
        public AiKernelTest() : base()
        {
        }

        [TestMethod]
        public async Task RunTest()
        {
            AgentAiHandler agentAiHandler = new AgentAiHandler(_senparcAiSetting);

            var iWantToRun =
            agentAiHandler.IWantTo()
                        .ConfigModel(ConfigModel.Chat, "Jeffrey")
                        .BuildKernel();

            var result = await iWantToRun.Kernel.RunAsync("给我介绍一下苏州园林");
            Assert.IsTrue(result.CreatedAt.Value.UtcDateTime > DateTime.UtcNow.AddMinutes(-5), "Result should be created within the last 5 minutes.");
            Console.WriteLine($"Result: {result.Text}");
            Console.WriteLine("Usage:" + result.Usage.ToJson(true));
        }
    }
}
