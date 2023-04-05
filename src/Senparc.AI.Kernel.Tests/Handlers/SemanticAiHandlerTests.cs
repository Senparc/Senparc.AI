using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Entities;
using Senparc.AI.Kernel.Helpers;
using Senparc.AI.Kernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Tests.Handlers
{
    [TestClass]
    public class SemanticAiHandlerTest : KernelTestBase
    {
        [TestMethod]
        public async Task ChatAsyncTest()
        {
            var helper = new SemanticKernelHelper();
            var handler = new SemanticAiHandler(helper);

            var prompt = "Where is the Tai Lake?";
            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var iWantToRun = await handler.ChatConfigAsync(parameter, userId: "Jeffrey", modelName: "text-davinci-003");

            var request = new SenparcAiRequest("Jeffrey", "text-davinci-003", prompt, parameter);
            var result = await handler.ChatAsync(iWantToRun, request);

            Assert.IsNotNull(result);
            Console.WriteLine(result.ToJson(true));
            Assert.IsNotNull(result.Output);

            Assert.IsTrue(result.Output.Length > 0);
            Assert.IsTrue(result.LastException == null);

        }
    }
}
