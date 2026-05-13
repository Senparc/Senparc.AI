using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Tests.Handlers
{
    [TestClass]
    public class AgentAiHandlerTests: KernelTestBase
    {
        [TestMethod]
        public void CreateTest()
        {
            var setting = Senparc.AI.Config.SenparcAiSetting;
            var agentAiHandler = new AgentAiHandler(setting);
            Assert.AreSame(setting, agentAiHandler.AgentKernelHelper.AiSetting);
        }

    }
}
