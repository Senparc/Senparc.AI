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
            var response =await  new AgentAiHandler(_senparcAiSetting)
                                        .IWantTo()
                                        .ConfigModel(ConfigModel.Chat, "Jeffrey")
                                        .BuildKernel()
                                        .RunChatAsync("You are an assistant responsible for answering my questions. Use at most 10 words per line. Introduce Suzhou gardens to me.");

            var result = response.Result;

            Assert.IsTrue(result.CreatedAt.Value.UtcDateTime > DateTime.UtcNow.AddMinutes(-5), "Result should be created within the last 5 minutes.");

            Console.WriteLine($"Result: {result.Text}");

            Console.WriteLine("Usage:" + result.Usage.ToJson(true));

        }
    }
}
