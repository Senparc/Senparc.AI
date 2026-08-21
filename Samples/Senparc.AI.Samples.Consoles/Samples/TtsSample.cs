using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.TextToAudio;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.Consoles.Samples
{
    /// <summary>
    /// Text-to-speech sample using a TTS model.
    /// </summary>
    public class TtsSample
    {
        private readonly IServiceProvider _serviceProvider;
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;

        public TtsSample(IServiceProvider serviceProvider, IAiHandler aiHandler)
        {
            this._serviceProvider = serviceProvider;
            _aiHandler = aiHandler;
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);
        }

        public async Task RunAsync()
        {
            var ttsSetting = Senparc.AI.Config.SenparcAiSetting;
            
            // Check API key configuration.
            if ((
                    ttsSetting.OpenAIKeys == null ||
                    ttsSetting.OpenAIKeys.ApiKey.IsNullOrEmpty()
                ) &&
                (
                    ttsSetting.AzureOpenAIKeys == null ||
                    ttsSetting.AzureOpenAIKeys.ApiKey.IsNullOrEmpty()
                ) &&
                (
                    ttsSetting.AiPlatform != AiPlatform.OpenAI && ttsSetting.AiPlatform != AiPlatform.AzureOpenAI
                )
               )
            {
                await Console.Out.WriteLineAsync("The TTS API requires an OpenAI or Azure OpenAI API key before use.");
                return;
            }

            await Console.Out.WriteLineAsync("========================================");
            await Console.Out.WriteLineAsync("TTS text-to-speech sample started");
            await Console.Out.WriteLineAsync("========================================");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("[Voice options]");
            await Console.Out.WriteLineAsync("  1. alloy   - neutral voice");
            await Console.Out.WriteLineAsync("  2. echo    - male voice");
            await Console.Out.WriteLineAsync("  3. fable   - British accent");
            await Console.Out.WriteLineAsync("  4. onyx    - deep male voice");
            await Console.Out.WriteLineAsync("  5. nova    - female voice");
            await Console.Out.WriteLineAsync("  6. shimmer - soft female voice");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("[Output format]");
            await Console.Out.WriteLineAsync("  mp3  - most common format with good compatibility");
            await Console.Out.WriteLineAsync("  opus - high compression ratio, suitable for network transmission");
            await Console.Out.WriteLineAsync("  aac  - suitable for streaming media");
            await Console.Out.WriteLineAsync("  flac - lossless audio quality");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("[Speech speed range]");
            await Console.Out.WriteLineAsync("  0.25x - 4.0x (default 1.0x)");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("Enter 'exit' to leave the program.");
            await Console.Out.WriteLineAsync();

            var userId = "Jeffrey";
            var iWantTo = _semanticAiHandler.IWantTo(ttsSetting)
                                .ConfigModel(ConfigModel.TextToSpeech, userId)
                                .BuildKernel();

            var textToAudioService = iWantTo.GetRequiredService<ITextToAudioService>();

            string inputText;
            while (true)
            {
                await Console.Out.WriteLineAsync("Enter the text to convert to speech, or enter 'exit' to leave:");
                inputText = Console.ReadLine() ?? "";
                
                if (inputText.ToLower() == "exit")
                {
                    break;
                }

                if (inputText.IsNullOrEmpty())
                {
                    await Console.Out.WriteLineAsync("Please enter valid text.");
                    await Console.Out.WriteLineAsync();
                    continue;
                }

                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("Select a voice (enter 1-6, default 1-alloy):");
                var voiceInput = Console.ReadLine() ?? "1";
                
                var voice = voiceInput switch
                {
                    "1" => "alloy",
                    "2" => "echo",
                    "3" => "fable",
                    "4" => "onyx",
                    "5" => "nova",
                    "6" => "shimmer",
                    _ => "alloy"
                };

                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("Select an output format (enter 1-4, default 1-mp3):");
                await Console.Out.WriteLineAsync("  1. mp3   - MP3 format (recommended, good compatibility)");
                await Console.Out.WriteLineAsync("  2. opus  - Opus format (high compression ratio)");
                await Console.Out.WriteLineAsync("  3. aac   - AAC format (suitable for streaming media)");
                await Console.Out.WriteLineAsync("  4. flac  - FLAC format (lossless audio quality)");
                var formatInput = Console.ReadLine() ?? "1";
                
                var format = formatInput switch
                {
                    "1" => "mp3",
                    "2" => "opus",
                    "3" => "aac",
                    "4" => "flac",
                    _ => "mp3"
                };

                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("Select speech speed (enter 1-7, default 4-normal 1.0x):");
                await Console.Out.WriteLineAsync("  1. 0.25x - extremely slow");
                await Console.Out.WriteLineAsync("  2. 0.5x  - very slow");
                await Console.Out.WriteLineAsync("  3. 0.75x - slow");
                await Console.Out.WriteLineAsync("  4. 1.0x  - normal (recommended)");
                await Console.Out.WriteLineAsync("  5. 1.25x - fast");
                await Console.Out.WriteLineAsync("  6. 1.5x  - very fast");
                await Console.Out.WriteLineAsync("  7. 2.0x  - extremely fast");
                await Console.Out.WriteLineAsync("  8. custom (0.25 - 4.0)");
                var speedInput = Console.ReadLine() ?? "4";
                
                float speed = speedInput switch
                {
                    "1" => 0.25f,
                    "2" => 0.5f,
                    "3" => 0.75f,
                    "4" => 1.0f,
                    "5" => 1.25f,
                    "6" => 1.5f,
                    "7" => 2.0f,
                    "8" => await GetCustomSpeedAsync(),
                    _ => 1.0f
                };

                try
                {
                    await Console.Out.WriteLineAsync();
                    await Console.Out.WriteLineAsync($"Generating speech (voice: {voice}, format: {format}, speed: {speed}x). Please wait...");
                    await Console.Out.WriteLineAsync();

                    // Create execution settings.
                    var executionSettings = new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAITextToAudioExecutionSettings
                    {
                        Voice = voice,
                        ResponseFormat = format,
                        Speed = speed
                    };

                    // Call the TTS API for text-to-speech.
                    var audioContent = await textToAudioService.GetAudioContentAsync(inputText, executionSettings);

                    // Save the audio file.
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var outputFileName = $"TTS-Output-{voice}-{speed}x-{timestamp}.{format}";
                    var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);

                    await Console.Out.WriteLineAsync($"[Debug] Audio data size: {audioContent.Data?.Length ?? 0} bytes");
                    await Console.Out.WriteLineAsync($"[Debug] Parameters - voice: {voice}, format: {format}, speed: {speed}x");

                    if (audioContent.Data.HasValue && audioContent.Data.Value.Length > 0)
                    {
                        await File.WriteAllBytesAsync(outputFilePath, audioContent.Data.Value.ToArray());
                        
                        await Console.Out.WriteLineAsync("========================================");
                        await Console.Out.WriteLineAsync("Speech generation succeeded.");
                        await Console.Out.WriteLineAsync($"Voice: {voice}");
                        await Console.Out.WriteLineAsync($"Format: {format}");
                        await Console.Out.WriteLineAsync($"Speed: {speed}x");
                        await Console.Out.WriteLineAsync($"Save path: {outputFilePath}");
                        await Console.Out.WriteLineAsync($"File size: {new FileInfo(outputFilePath).Length} bytes");
                        await Console.Out.WriteLineAsync("========================================");
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("Error: failed to get audio data.");
                    }
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
            }

            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("TTS sample exited.");
        }

        /// <summary>
        /// Gets a custom speech speed from the user.
        /// </summary>
        /// <returns></returns>
        private async Task<float> GetCustomSpeedAsync()
        {
            await Console.Out.WriteLineAsync("Enter a custom speech speed (0.25 - 4.0):");
            var customSpeedInput = Console.ReadLine() ?? "1.0";
            
            if (float.TryParse(customSpeedInput, out var customSpeed))
            {
                // Clamp the value to the 0.25 to 4.0 range.
                if (customSpeed < 0.25f)
                {
                    await Console.Out.WriteLineAsync("Speech speed is too slow; adjusted to the minimum value 0.25x.");
                    return 0.25f;
                }
                else if (customSpeed > 4.0f)
                {
                    await Console.Out.WriteLineAsync("Speech speed is too fast; adjusted to the maximum value 4.0x.");
                    return 4.0f;
                }
                return customSpeed;
            }
            else
            {
                await Console.Out.WriteLineAsync("Invalid input; using default speed 1.0x.");
                return 1.0f;
            }
        }
    }
}
