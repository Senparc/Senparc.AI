using Microsoft.Agents.AI;
using Senparc.AI.AgentKernel.Harness;

namespace Senparc.AI.AgentKernel.Tests.Harness;

[TestClass]
public class AgentKernelHarnessOptionsTests
{
    [TestMethod]
    public void CreateLeastPrivilegeOptions_DisablesAmbientCapabilities()
    {
        var options = AgentKernelHarnessOptions.CreateLeastPrivilegeOptions();

        Assert.IsTrue(options.DisableFileAccess);
        Assert.IsTrue(options.DisableFileMemory);
        Assert.IsTrue(options.DisableAgentSkillsProvider);
        Assert.IsTrue(options.DisableWebSearch);
        Assert.IsFalse(options.DisableTodoProvider);
        Assert.IsFalse(options.DisableAgentModeProvider);
    }

    [TestMethod]
    public void Defaults_KeepCompactionLimitsValid()
    {
        var options = new AgentKernelHarnessOptions();

        Assert.IsTrue(options.MaxContextWindowTokens > 0);
        Assert.IsTrue(options.MaxOutputTokens >= 0);
        Assert.IsTrue(options.MaxOutputTokens < options.MaxContextWindowTokens);
    }
}
