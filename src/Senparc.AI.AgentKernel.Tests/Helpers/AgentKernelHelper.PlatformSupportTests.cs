using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Exceptions;
using System.Net.Http;

namespace Senparc.AI.AgentKernel.Tests.Helpers
{
    [TestClass]
    public class AgentKernelHelperPlatformSupportTests
    {
        [DataTestMethod]
        [DataRow(AiPlatform.HuggingFace)]
        [DataRow(AiPlatform.FastAPI)]
        [DataRow(AiPlatform.DeepSeek)]
        [DataRow(AiPlatform.Anthropic)]
        [DataRow(AiPlatform.Gemini)]
        [DataRow(AiPlatform.Qwen)]
        [DataRow(AiPlatform.Kimi)]
        [DataRow(AiPlatform.XunFei)]
        public void ConfigChat_ShouldCreateChatClient_ForSupportedAdditionalPlatforms(AiPlatform platform)
        {
            var setting = CreateSetting(platform);
            var helper = new AgentKernelHelper(setting, httpClient: new HttpClient());

            var builder = helper.ConfigChat("UnitTestUser", senparcAiSetting: setting);

            Assert.IsNotNull(builder.ChatClient);
            if (platform is AiPlatform.HuggingFace or AiPlatform.FastAPI)
            {
                Assert.IsInstanceOfType(builder.ChatClient, typeof(IChatClient));
            }
            else
            {
                Assert.IsInstanceOfType(builder.ChatClient, typeof(ChatClient));
            }

            Assert.IsTrue(builder.ConfigModels.Contains(ConfigModel.Chat));
        }

        [DataTestMethod]
        [DataRow(AiPlatform.HuggingFace)]
        [DataRow(AiPlatform.FastAPI)]
        [DataRow(AiPlatform.DeepSeek)]
        [DataRow(AiPlatform.Gemini)]
        [DataRow(AiPlatform.Qwen)]
        [DataRow(AiPlatform.Kimi)]
        public void ConfigTextEmbeddingGeneration_ShouldCreateEmbeddingClient_ForSupportedPlatforms(AiPlatform platform)
        {
            var setting = CreateSetting(platform);
            var helper = new AgentKernelHelper(setting, httpClient: new HttpClient());

            var builder = helper.ConfigTextEmbeddingGeneration("UnitTestUser", senparcAiSetting: setting);

            Assert.IsNotNull(builder.EmbeddingClient);
            if (platform is AiPlatform.HuggingFace or AiPlatform.FastAPI)
            {
                Assert.IsInstanceOfType(builder.EmbeddingClient, typeof(IEmbeddingGenerator));
            }
            else
            {
                Assert.IsInstanceOfType(builder.EmbeddingClient, typeof(EmbeddingClient));
            }

            Assert.IsTrue(builder.ConfigModels.Contains(ConfigModel.TextEmbedding));
        }

        [TestMethod]
        public void ConfigTextEmbeddingGeneration_Anthropic_ShouldThrow()
        {
            var setting = CreateSetting(AiPlatform.Anthropic);
            var helper = new AgentKernelHelper(setting, httpClient: new HttpClient());

            Assert.ThrowsExactly<SenparcAiException>(() =>
                helper.ConfigTextEmbeddingGeneration("UnitTestUser", senparcAiSetting: setting));
        }

        [TestMethod]
        public void ConfigTextEmbeddingGeneration_XunFei_ShouldThrow()
        {
            var setting = CreateSetting(AiPlatform.XunFei);
            var helper = new AgentKernelHelper(setting, httpClient: new HttpClient());

            Assert.ThrowsExactly<SenparcAiException>(() =>
                helper.ConfigTextEmbeddingGeneration("UnitTestUser", senparcAiSetting: setting));
        }

        private static SenparcAiSetting CreateSetting(AiPlatform platform)
        {
            var chatModelName = $"{platform.ToString().ToLowerInvariant()}-chat";
            var embeddingModelName = $"{platform.ToString().ToLowerInvariant()}-embedding";
            var modelName = new ModelName
            {
                Chat = chatModelName,
                Embedding = embeddingModelName,
                EmbeddingDimensions = 1536
            };
            var setting = new SenparcAiSetting();

            switch (platform)
            {
                case AiPlatform.HuggingFace:
                    setting.SetHuggingFace(new HuggingFaceKeys
                    {
                        ApiKey = "hf-key",
                        Endpoint = "https://router.huggingface.co/v1",
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.FastAPI:
                    setting.SetFastAPI(new FastAPIKeys
                    {
                        ApiKey = "fastapi-key",
                        Endpoint = "https://fastapi.example.com/v1",
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.DeepSeek:
                    setting.SetDeepSeek(new DeepSeekKeys
                    {
                        ApiKey = "deepseek-key",
                        Endpoint = "https://api.deepseek.com/v1",
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.Anthropic:
                    setting.SetAnthropic(new AnthropicKeys
                    {
                        ApiKey = "anthropic-key",
                        Endpoint = "https://api.anthropic.com",
                        AnthropicVersion = "2023-06-01",
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.Gemini:
                    setting.SetGemini(new GeminiKeys
                    {
                        ApiKey = "gemini-key",
                        Endpoint = "https://generativelanguage.googleapis.com",
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.Qwen:
                    setting.SetQwen(new QwenKeys
                    {
                        ApiKey = "qwen-key",
                        Endpoint = "https://dashscope.aliyuncs.com/compatible-mode/v1",
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.Kimi:
                    setting.SetKimi(new KimiKeys
                    {
                        ApiKey = "kimi-key",
                        Endpoint = "https://api.moonshot.ai/v1",
                        ModelName = modelName
                    });
                    break;
                case AiPlatform.XunFei:
                    setting.SetXunFei(new XunFeiKeys
                    {
                        ApiKey = "xunfei-key",
                        Endpoint = "https://maas-api.cn-huabei-1.xf-yun.com/v1",
                        ModelName = modelName
                    });
                    break;
                default:
                    throw new AssertFailedException($"Unsupported platform in unit test: {platform}");
            }

            return setting;
        }
    }
}
