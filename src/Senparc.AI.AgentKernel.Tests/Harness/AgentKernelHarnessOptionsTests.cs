using Microsoft.Agents.AI;
using Senparc.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Harness;
using Senparc.AI.Entities.Keys;

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

    [TestMethod]
    public void SupportsStreamingFor_NeuCharAI_UsesCompatibleNonStreamingTransport()
    {
        var setting = new SenparcAiSetting
        {
            AiPlatform = AiPlatform.NeuCharAI,
            NeuCharAIKeys = new NeuCharAIKeys
            {
                ModelName = new ModelName
                {
                    Chat = "gpt-5.6-luna"
                }
            }
        };

        Assert.IsFalse(AgentKernelHarness.SupportsStreamingFor(setting));
        setting.AiPlatform = AiPlatform.OpenAI;
        Assert.IsTrue(AgentKernelHarness.SupportsStreamingFor(setting));
    }
}
