using Microsoft.Extensions.Configuration;
using Senparc.AI.AgentKernel.Mcp;
using Senparc.AI.Interfaces;

namespace Senparc.AI.AgentKernel.Tests.Helpers;

[TestClass]
public class McpToolsetBuilderTests
{
    [TestMethod]
    public void ResolveSseUrl_UsePublicBaseUrl_PreservesPath()
    {
        var option = new McpServerOption
        {
            Name = "Demo",
            ServerName = "demo",
            LocalSseUrl = "http://127.0.0.1:5000/mcp/sse",
            PublicBaseUrl = "https://abc.trycloudflare.com"
        };

        var resolved = McpToolsetBuilder.ResolveSseUrl(option);

        Assert.AreEqual("https://abc.trycloudflare.com/mcp/sse", resolved);
    }

    [TestMethod]
    public void TryMergePublicBaseUrl_PreserveQuery_ReturnsExpected()
    {
        var ok = McpToolsetBuilder.TryMergePublicBaseUrl(
            "https://abc.trycloudflare.com",
            "http://127.0.0.1:5000/mcp/sse?x=1&y=2",
            out var merged,
            out var error);

        Assert.IsTrue(ok, error);
        Assert.AreEqual("https://abc.trycloudflare.com/mcp/sse?x=1&y=2", merged);
    }

    [TestMethod]
    public void IsLocalAddress_Loopback_ReturnsTrue()
    {
        Assert.IsTrue(McpToolsetBuilder.IsLocalAddress("http://127.0.0.1:5000/mcp/sse"));
        Assert.IsTrue(McpToolsetBuilder.IsLocalAddress("http://localhost:5000/mcp/sse"));
        Assert.IsFalse(McpToolsetBuilder.IsLocalAddress("https://abc.trycloudflare.com/mcp/sse"));
    }

    [TestMethod]
    public void CreateChatClientAgentOptions_EmptyTools_StillBuilds()
    {
        var options = McpToolsetBuilder.CreateChatClientAgentOptions([]);

        Assert.IsNotNull(options.ChatOptions);
        Assert.AreEqual(McpToolsetBuilder.DefaultSystemPrompt, options.ChatOptions.Instructions);
        Assert.AreEqual(0, options.ChatOptions.Tools.Count);
        Assert.IsTrue(options.ChatOptions.AllowMultipleToolCalls);
    }

    [TestMethod]
    public void GetMcpServerOptions_ReadFromConfiguration_ReturnsServers()
    {
        var data = new Dictionary<string, string?>
        {
            ["SenparcAiSetting:McpServers:0:Name"] = "Local",
            ["SenparcAiSetting:McpServers:0:ServerName"] = "local-demo",
            ["SenparcAiSetting:McpServers:0:LocalSseUrl"] = "http://127.0.0.1:5000/mcp/sse",
            ["SenparcAiSetting:McpServers:0:ToolBindingMode"] = "LocalFunctionProxy",
            ["SenparcAiSetting:McpServers:0:AllowedTools:0"] = "echo",
            ["SenparcAiSetting:McpServers:0:AllowedTools:1"] = "now",
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();

        var options = config.GetMcpServerOptions();

        Assert.AreEqual(1, options.Count);
        Assert.AreEqual("Local", options[0].Name);
        Assert.AreEqual("local-demo", options[0].ServerName);
        Assert.AreEqual(2, options[0].AllowedTools.Count);
        Assert.AreEqual(McpToolBindingMode.LocalFunctionProxy, options[0].GetBindingMode());
    }
}
