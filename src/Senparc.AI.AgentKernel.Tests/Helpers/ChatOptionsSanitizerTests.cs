using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Helpers;

namespace Senparc.AI.AgentKernel.Tests.Helpers;

[TestClass]
public class ChatOptionsSanitizerTests
{
    [TestMethod]
    public void SanitizeForModel_Gpt5_RemovesTemperatureAndTopP()
    {
        var options = new ChatOptions
        {
            Temperature = 0.2f,
            TopP = 0.2f,
            MaxOutputTokens = 2000
        };

        var removed = ChatOptionsSanitizer.SanitizeForModel(options, "gpt-5.6-sol");

        Assert.IsTrue(removed);
        Assert.IsNull(options.Temperature);
        Assert.IsNull(options.TopP);
        Assert.AreEqual(2000, options.MaxOutputTokens);
    }

    [TestMethod]
    public void SanitizeForModel_Gpt4o_KeepsTemperature()
    {
        var options = new ChatOptions
        {
            Temperature = 0.7f,
            TopP = 0.9f
        };

        var removed = ChatOptionsSanitizer.SanitizeForModel(options, "gpt-4o");

        Assert.IsFalse(removed);
        Assert.AreEqual(0.7f, options.Temperature);
        Assert.AreEqual(0.9f, options.TopP);
    }

    [TestMethod]
    public void SanitizeForModel_NullOptions_ReturnsFalse()
    {
        Assert.IsFalse(ChatOptionsSanitizer.SanitizeForModel(null, "gpt-5"));
    }

    [TestMethod]
    public void SanitizeForModel_Gpt5_AlreadyNull_ReturnsFalse()
    {
        var options = new ChatOptions
        {
            Instructions = "hi"
        };

        var removed = ChatOptionsSanitizer.SanitizeForModel(options, "gpt-5");

        Assert.IsFalse(removed);
        Assert.IsNull(options.Temperature);
    }
}
