using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenAI.Audio;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using System.IO;
using System.Linq;

namespace Senparc.AI.AgentKernel.Tests.KernelConfigExtensions
{
    [TestClass]
    public class KernelConfigExtensionsSpeechTests : KernelTestBase
    {
        [TestMethod]
        public void ConfigSpeechToTextModel_ShouldConfigureSpeechToTextClient()
        {
            var handler = new AgentAiHandler(_senparcAiSetting);
            var iWantToRun = handler.IWantTo()
                .ConfigSpeechToTextModel("Jeffrey")
                .BuildKernel();

            var kernel = iWantToRun.Kernel;
            Assert.IsNotNull(kernel);
            Assert.IsTrue(kernel.ConfigModels.Contains(ConfigModel.SpeechToText));
            Assert.IsNotNull(kernel.SpeechToTextClient);
            Assert.IsTrue(kernel.SpeechToTextClient is AudioClient);
        }

        [TestMethod]
        public void ConfigTextToSpeechModel_ShouldConfigureTextToSpeechClient()
        {
            var handler = new AgentAiHandler(_senparcAiSetting);
            var iWantToRun = handler.IWantTo()
                .ConfigTextToSpeechModel("Jeffrey")
                .BuildKernel();

            var kernel = iWantToRun.Kernel;
            Assert.IsNotNull(kernel);
            Assert.IsTrue(kernel.ConfigModels.Contains(ConfigModel.TextToSpeech));
            Assert.IsNotNull(kernel.TextToSpeechClient);
            Assert.IsTrue(kernel.TextToSpeechClient is AudioClient);
        }

        [TestMethod]
        public void ConfigModel_SpeechToText_And_TextToSpeech_ShouldConfigureBoth()
        {
            var handler = new AgentAiHandler(_senparcAiSetting);
            var iWantToRun = handler.IWantTo()
                .ConfigSpeechToTextModel("Jeffrey")
                .ConfigTextToSpeechModel("Jeffrey")
                .BuildKernel();

            var kernel = iWantToRun.Kernel;
            Assert.IsTrue(kernel.ConfigModels.Contains(ConfigModel.SpeechToText));
            Assert.IsTrue(kernel.ConfigModels.Contains(ConfigModel.TextToSpeech));
            Assert.IsNotNull(kernel.SpeechToTextClient);
            Assert.IsNotNull(kernel.TextToSpeechClient);
        }

        [TestMethod]
        public void ParseGeneratedSpeechVoice_UnknownValue_ShouldFallbackToAlloy()
        {
            var voice = Senparc.AI.AgentKernel.Handlers.KernelConfigExtensions.ParseGeneratedSpeechVoice("unknown-voice");
            Assert.AreEqual(GeneratedSpeechVoice.Alloy, voice);
        }

        [TestMethod]
        public void ParseGeneratedSpeechFormat_UnknownValue_ShouldFallbackToMp3()
        {
            var format = Senparc.AI.AgentKernel.Handlers.KernelConfigExtensions.ParseGeneratedSpeechFormat("unknown-format");
            Assert.AreEqual(GeneratedSpeechFormat.Mp3, format);
        }

        [TestMethod]
        public async Task SpeechToTextAsync_WithoutSpeechConfig_ShouldThrow()
        {
            var handler = new AgentAiHandler(_senparcAiSetting);
            var iWantToRun = handler.IWantTo()
                .ConfigSpeechToTextModel("Jeffrey")
                .BuildKernel();

            //await Assert.ThrowsAsync<Exception>(async () =>
            //    await iWantToRun.Kernel.SpeechToTextAsync(new MemoryStream([1, 2, 3]), "sample.mp3"));

            var audioFilePath = "../../../STT-Test.m4a";
            var result = await iWantToRun.Kernel.SpeechToTextAsync(audioFilePath);
            Assert.AreEqual("你好,1234567890", result.Value.Text);
            Console.WriteLine(result.Value.Text);

        }

        [TestMethod]
        public async Task TextToSpeechAsync_WithoutSpeechConfig_ShouldThrow()
        {
            var handler = new AgentAiHandler(_senparcAiSetting);
            var iWantToRun = handler.IWantTo()
                .ConfigTextToSpeechModel("Jeffrey")
                .BuildKernel();

            //await Assert.ThrowsExceptionAsync<Exception>(async () =>
            //    await iWantToRun.Kernel.TextToSpeechAsync("hello", GeneratedSpeechVoice.Alloy));

#pragma warning disable OPENAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            System.ClientModel.ClientResult<BinaryData>? result = await iWantToRun.Kernel.TextToSpeechAsync("hello, welcome to use Senparc.AI.", GeneratedSpeechVoice.Alloy, new SpeechGenerationOptions()
            {
                SpeedRatio = 0.8f, Instructions =  "Use a friendly tone. Emphasize the word 'welcome'."
            });
#pragma warning restore OPENAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            Assert.IsNotNull(result);
            var outputPath = "output.mp3";
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                await result.Value.ToStream().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
            Console.WriteLine($"Audio saved to {outputPath}");
        }
    }
}
