using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;

namespace Senparc.AI.AgentKernel.Tests;

[TestClass]
public class AgentKernelHelperTests:KernelTestBase
{
    [TestMethod]
    public void CreateTest()
    {
        var setting = Senparc.AI.Config.SenparcAiSetting;
        var agentKernelHelper= new AgentKernelHelper(setting);
        Assert.AreSame(setting, agentKernelHelper.AiSetting);
    }
}
