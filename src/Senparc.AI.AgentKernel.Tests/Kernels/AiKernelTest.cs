using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.CO2NET.Extensions;
using Microsoft.Extensions.DependencyInjection;

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
            var result =await  new AgentAiHandler(_senparcAiSetting)
                                        .IWantTo()
                                        .ConfigModel(ConfigModel.Chat, "Jeffrey")
                                        .BuildKernel().Kernel
                                        .RunAsync("你是一个助理负责回答我的问题，每行最多10个字。给我介绍一下苏州园林");


            Assert.IsTrue(result.CreatedAt.Value.UtcDateTime > DateTime.UtcNow.AddMinutes(-5), "Result should be created within the last 5 minutes.");

            Console.WriteLine($"Result: {result.Text}");

            Console.WriteLine("Usage:" + result.Usage.ToJson(true));

        }
    }
}
