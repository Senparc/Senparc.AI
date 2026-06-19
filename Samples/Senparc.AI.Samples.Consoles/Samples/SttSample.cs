using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.Consoles.Samples
{
    /// <summary>
    /// Speech-to-text sample using the Whisper model.
    /// </summary>
    public class SttSample
    {
        private readonly IServiceProvider _serviceProvider;
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public SttSample(IServiceProvider serviceProvider, IAiHandler aiHandler)
        {
            this._serviceProvider = serviceProvider;
            _aiHandler = aiHandler;
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);
        }

        public async Task RunAsync()
        {
            var sttSetting = Senparc.AI.Config.SenparcAiSetting;
            
            // Check API key configuration.
            if ((
                    sttSetting.OpenAIKeys == null ||
                    sttSetting.OpenAIKeys.ApiKey.IsNullOrEmpty()
                ) &&
                (
                    sttSetting.AzureOpenAIKeys == null ||
                    sttSetting.AzureOpenAIKeys.ApiKey.IsNullOrEmpty()
                ) &&
                (
                    sttSetting.AiPlatform != AiPlatform.OpenAI && sttSetting.AiPlatform != AiPlatform.AzureOpenAI
                )
               )
            {
                await Console.Out.WriteLineAsync("The Whisper API requires an OpenAI or Azure OpenAI API key before use.");
                return;
            }

            await Console.Out.WriteLineAsync("========================================");
            await Console.Out.WriteLineAsync("Whisper speech-to-text (STT) sample started");
            await Console.Out.WriteLineAsync("========================================");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("Enter an audio file path. Supported formats: flac, m4a, mp3, mp4, mpeg, mpga, oga, ogg, wav, webm.");
            await Console.Out.WriteLineAsync("Enter 'exit' to leave the program.");
            await Console.Out.WriteLineAsync();

            var userId = "Jeffrey";
            var iWantTo = _semanticAiHandler.IWantTo(sttSetting)
                                .ConfigModel(ConfigModel.SpeechToText, userId)
                                .BuildKernel();

            var audioToTextService = iWantTo.GetRequiredService<IAudioToTextService>();

            string audioFilePath;
            while ((audioFilePath = Console.ReadLine() ?? "") != "exit")
            {
                Console.WriteLine();
                
                if (audioFilePath.IsNullOrEmpty())
                {
                    await Console.Out.WriteLineAsync("Please enter a valid file path.");
                    await Console.Out.WriteLineAsync();
                    continue;
                }

                // Check whether the file exists.
                if (!File.Exists(audioFilePath))
                {
                    await Console.Out.WriteLineAsync($"File not found: {audioFilePath}");
                    await Console.Out.WriteLineAsync();
                    continue;
                }

                try
                {
                    await Console.Out.WriteLineAsync("Converting audio to text. Please wait...");
                    await Console.Out.WriteLineAsync();

                    // Read the audio file.
                    await using var audioFileStream = File.OpenRead(audioFilePath);
                    var audioFileBinaryData = await BinaryData.FromStreamAsync(audioFileStream);
                    var fileName = Path.GetFileName(audioFilePath);
                    var fileExtension = Path.GetExtension(audioFilePath).ToLower();

                    await Console.Out.WriteLineAsync($"[Debug] File name: {fileName}, extension: {fileExtension}");

                    // Create audio content; use a null MIME type so the SDK can infer it.
                    var audioContent = new Microsoft.SemanticKernel.AudioContent(audioFileBinaryData, mimeType: null);
                    
                    // Create execution settings; the file name is required.
                    var executionSettings = new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIAudioToTextExecutionSettings(fileName)
                    {
                        Language = "zh" // Use "en" for English recognition.
                    };

                    // Call the Whisper API for speech-to-text.
                    var textContent = await audioToTextService.GetTextContentAsync(audioContent, executionSettings);

                    await Console.Out.WriteLineAsync("========================================");
                    await Console.Out.WriteLineAsync("Conversion result:");
                    await Console.Out.WriteLineAsync("========================================");
                    await Console.Out.WriteLineAsync(textContent.Text);
                    await Console.Out.WriteLineAsync("========================================");
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Conversion failed: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        await Console.Out.WriteLineAsync($"Detailed error: {ex.InnerException.Message}");
                    }
                }

                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("Enter another audio file path, or enter 'exit' to leave:");
            }

            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("STT sample exited.");
        }
    }
}
