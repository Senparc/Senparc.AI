using Senparc.AI.Entities.Keys;

namespace Senparc.AI.AgentKernel.Tests.Helpers
{
    [TestClass]
    public class SenparcAiSettingAdditionalPlatformTests
    {
        [DataTestMethod]
        [DataRow(AiPlatform.Anthropic)]
        [DataRow(AiPlatform.Gemini)]
        [DataRow(AiPlatform.Qwen)]
        [DataRow(AiPlatform.Kimi)]
        [DataRow(AiPlatform.XunFei)]
        public void AdditionalPlatforms_ShouldResolveProviderSpecificProperties(AiPlatform platform)
        {
            var endpoint = $"https://{platform.ToString().ToLowerInvariant()}.example.com/v1";
            var apiKey = $"{platform}-key";

            var setting = CreateSetting(platform, endpoint, apiKey);

            Assert.AreEqual(platform, setting.AiPlatform);
            Assert.AreEqual(endpoint, setting.Endpoint);
            Assert.AreEqual(apiKey, setting.ApiKey);
            Assert.IsNull(setting.OrganizationId);
            Assert.AreEqual("chat-model", setting.ModelName.Chat);
            Assert.AreEqual("embedding-model", setting.ModelName.Embedding);
            Assert.IsNull(setting.DeploymentName);

            if (platform == AiPlatform.Anthropic)
            {
                Assert.AreEqual("2023-06-01", setting.AnthropicVersion);
            }
        }

        private static SenparcAiSetting CreateSetting(AiPlatform platform, string endpoint, string apiKey)
        {
            var modelName = new ModelName
            {
                Chat = "chat-model",
                Embedding = "embedding-model"
            };

            var setting = new SenparcAiSetting();

            switch (platform)
            {
                case AiPlatform.Anthropic:
                    setting.SetAnthropic(new AnthropicKeys
                    {
                        ApiKey = apiKey,
                        Endpoint = endpoint,
                        AnthropicVersion = "2023-06-01",
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.Gemini:
                    setting.SetGemini(new GeminiKeys
                    {
                        ApiKey = apiKey,
                        Endpoint = endpoint,
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.Qwen:
                    setting.SetQwen(new QwenKeys
                    {
                        ApiKey = apiKey,
                        Endpoint = endpoint,
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.Kimi:
                    setting.SetKimi(new KimiKeys
                    {
                        ApiKey = apiKey,
                        Endpoint = endpoint,
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.XunFei:
                    setting.SetXunFei(new XunFeiKeys
                    {
                        ApiKey = apiKey,
                        Endpoint = endpoint,
                        ModelName = modelName
                    });
                    break;
                default:
                    throw new AssertFailedException($"Unsupported platform in test: {platform}");
            }

            return setting;
        }
    }
}
