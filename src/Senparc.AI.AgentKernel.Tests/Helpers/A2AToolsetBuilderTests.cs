using Microsoft.Extensions.Configuration;
using Senparc.AI.AgentKernel.A2A;
using Senparc.AI.Interfaces;

namespace Senparc.AI.AgentKernel.Tests.Helpers;

[TestClass]
public class A2AToolsetBuilderTests
{
    [TestMethod]
    public void ResolveBaseUrl_UsePublicBaseUrl_PreservesPath()
    {
        var option = new A2AAgentOption
        {
            Name = "Demo",
            LocalBaseUrl = "http://127.0.0.1:5000/a2a",
            PublicBaseUrl = "https://abc.trycloudflare.com"
        };

        var resolved = A2AToolsetBuilder.ResolveBaseUrl(option);

        Assert.AreEqual("https://abc.trycloudflare.com/a2a", resolved);
    }

    [TestMethod]
    public void TryMergePublicBaseUrl_PreserveQuery_ReturnsExpected()
    {
        var ok = A2AToolsetBuilder.TryMergePublicBaseUrl(
            "https://abc.trycloudflare.com",
            "http://127.0.0.1:5000/a2a?x=1&y=2",
            out var merged,
            out var error);

        Assert.IsTrue(ok, error);
        Assert.AreEqual("https://abc.trycloudflare.com/a2a?x=1&y=2", merged);
    }

    [TestMethod]
    public void IsLocalAddress_Loopback_ReturnsTrue()
    {
        Assert.IsTrue(A2AToolsetBuilder.IsLocalAddress("http://127.0.0.1:5000/a2a"));
        Assert.IsTrue(A2AToolsetBuilder.IsLocalAddress("http://localhost:5000/a2a"));
        Assert.IsFalse(A2AToolsetBuilder.IsLocalAddress("https://abc.trycloudflare.com/a2a"));
    }

    [TestMethod]
    public void CreateChatClientAgentOptions_EmptyTools_StillBuilds()
    {
        var options = A2AToolsetBuilder.CreateChatClientAgentOptions([]);

        Assert.IsNotNull(options.ChatOptions);
        Assert.AreEqual(A2AToolsetBuilder.DefaultSystemPrompt, options.ChatOptions.Instructions);
        Assert.AreEqual(0, options.ChatOptions.Tools.Count);
        Assert.IsTrue(options.ChatOptions.AllowMultipleToolCalls);
    }

    [TestMethod]
    public void GetA2AAgentOptions_ReadFromConfiguration_ReturnsAgents()
    {
        var data = new Dictionary<string, string?>
        {
            ["SenparcAiSetting:A2AAgents:0:Name"] = "Remote-Agent",
            ["SenparcAiSetting:A2AAgents:0:BaseUrl"] = "https://demo-agent.example.com/a2a",
            ["SenparcAiSetting:A2AAgents:0:ToolBindingMode"] = "LocalFunctionProxy",
            ["SenparcAiSetting:A2AAgents:0:PreferredBindings:0"] = "HTTP+JSON",
            ["SenparcAiSetting:A2AAgents:0:AllowedSkills:0"] = "calendar",
            ["SenparcAiSetting:A2AAgents:0:AllowedSkills:1"] = "search",
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();

        var options = config.GetA2AAgentOptions();

        Assert.AreEqual(1, options.Count);
        Assert.AreEqual("Remote-Agent", options[0].Name);
        Assert.AreEqual("https://demo-agent.example.com/a2a", options[0].BaseUrl);
        Assert.AreEqual(1, options[0].PreferredBindings.Count);
        Assert.AreEqual(2, options[0].AllowedSkills.Count);
        Assert.AreEqual(A2AToolBindingMode.LocalFunctionProxy, options[0].GetBindingMode());
    }
}
