using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Helpers;

namespace Senparc.AI.Tests.Helpers
{
    [TestClass]
    public class ModelCapabilityHelperTests
    {
        [TestMethod]
        [DataRow("gpt-5")]
        [DataRow("gpt-5.6-sol")]
        [DataRow("GPT-5-mini")]
        [DataRow("gpt-5-chat")]
        [DataRow("openai/gpt-5.6-sol")]
        [DataRow("o1")]
        [DataRow("o1-mini")]
        [DataRow("o3-mini")]
        [DataRow("o4-mini")]
        public void DoesNotSupportTemperature_Gpt5AndOSeries_ReturnsTrue(string modelName)
        {
            Assert.IsTrue(ModelCapabilityHelper.DoesNotSupportTemperature(modelName));
            Assert.IsFalse(ModelCapabilityHelper.SupportsTemperature(modelName));
        }

        [TestMethod]
        [DataRow("gpt-4o")]
        [DataRow("gpt-4.1")]
        [DataRow("gpt-35-turbo")]
        [DataRow("deepseek-chat")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void DoesNotSupportTemperature_LegacyOrEmpty_ReturnsFalse(string? modelName)
        {
            Assert.IsFalse(ModelCapabilityHelper.DoesNotSupportTemperature(modelName));
            Assert.IsTrue(ModelCapabilityHelper.SupportsTemperature(modelName));
        }
    }
}
