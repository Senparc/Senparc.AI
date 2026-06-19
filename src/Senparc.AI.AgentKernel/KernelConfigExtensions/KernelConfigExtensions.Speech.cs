using OpenAI.Audio;
using Senparc.CO2NET.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.AI.AgentKernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        /// <summary>
        /// Runs speech-to-text and returns the recognized text.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="audioFilePath">Audio file path.</param>
        /// <param name="options">Recognition options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Recognized text.</returns>
        public static async Task<string> RunSpeechToTextAsync(
            this IWantToRun iWantToRun,
            string audioFilePath,
            AudioTranscriptionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var result = await iWantToRun.Kernel.SpeechToTextAsync(audioFilePath, options, cancellationToken);
            return result.Value?.Text ?? string.Empty;
        }

        /// <summary>
        /// Runs speech-to-text and returns the recognized text.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="audioStream">Audio stream.</param>
        /// <param name="audioFileName">Audio file name, including extension.</param>
        /// <param name="options">Recognition options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Recognized text.</returns>
        public static async Task<string> RunSpeechToTextAsync(
            this IWantToRun iWantToRun,
            Stream audioStream,
            string audioFileName,
            AudioTranscriptionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var result = await iWantToRun.Kernel.SpeechToTextAsync(audioStream, audioFileName, options, cancellationToken);
            return result.Value?.Text ?? string.Empty;
        }

        /// <summary>
        /// Runs text-to-speech and returns audio bytes.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="text">Input text.</param>
        /// <param name="voice">Voice, such as alloy, nova, or echo.</param>
        /// <param name="format">Output format, such as mp3, opus, or wav.</param>
        /// <param name="speedRatio">Speech speed. Recommended range is 0.5 to 2.0.</param>
        /// <param name="instructions">Additional speech style instructions.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Audio bytes.</returns>
        public static async Task<BinaryData> RunTextToSpeechAsync(
            this IWantToRun iWantToRun,
            string text,
            string voice = "alloy",
            string format = "mp3",
            float? speedRatio = null,
            string instructions = null,
            CancellationToken cancellationToken = default)
        {
            var parsedVoice = ParseGeneratedSpeechVoice(voice);
#pragma warning disable OPENAI001 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
            var options = new SpeechGenerationOptions
            {
                SpeedRatio = speedRatio,
                ResponseFormat = ParseGeneratedSpeechFormat(format),
                Instructions = instructions.IsNullOrEmpty() ? null : instructions
            };
#pragma warning restore OPENAI001 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.

            var result = await iWantToRun.Kernel.TextToSpeechAsync(text, parsedVoice, options, cancellationToken);
            return result.Value;
        }

        /// <summary>
        /// Runs text-to-speech and returns audio bytes.
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="text">Input text.</param>
        /// <param name="voice">Voice enum.</param>
        /// <param name="options">Generation options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Audio bytes.</returns>
        public static async Task<BinaryData> RunTextToSpeechAsync(
            this IWantToRun iWantToRun,
            string text,
            GeneratedSpeechVoice voice,
            SpeechGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var result = await iWantToRun.Kernel.TextToSpeechAsync(text, voice, options, cancellationToken);
            return result.Value;
        }

        /// <summary>
        /// Converts a string voice name to <see cref="GeneratedSpeechVoice"/> and returns Alloy when it is not recognized.
        /// </summary>
        public static GeneratedSpeechVoice ParseGeneratedSpeechVoice(string voice)
        {
#pragma warning disable OPENAI001 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
            return voice?.Trim().ToLowerInvariant() switch
            {
                "alloy" => GeneratedSpeechVoice.Alloy,
                "ash" => GeneratedSpeechVoice.Ash,
                "ballad" => GeneratedSpeechVoice.Ballad,
                "coral" => GeneratedSpeechVoice.Coral,
                "echo" => GeneratedSpeechVoice.Echo,
                "fable" => GeneratedSpeechVoice.Fable,
                "onyx" => GeneratedSpeechVoice.Onyx,
                "nova" => GeneratedSpeechVoice.Nova,
                "sage" => GeneratedSpeechVoice.Sage,
                "shimmer" => GeneratedSpeechVoice.Shimmer,
                "verse" => GeneratedSpeechVoice.Verse,
                _ => GeneratedSpeechVoice.Alloy
            };
#pragma warning restore OPENAI001 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
        }

        /// <summary>
        /// Converts a string audio format to <see cref="GeneratedSpeechFormat"/> and returns Mp3 when it is not recognized.
        /// </summary>
        public static GeneratedSpeechFormat ParseGeneratedSpeechFormat(string format)
        {
            return format?.Trim().ToLowerInvariant() switch
            {
                "mp3" => GeneratedSpeechFormat.Mp3,
                "opus" => GeneratedSpeechFormat.Opus,
                "aac" => GeneratedSpeechFormat.Aac,
                "flac" => GeneratedSpeechFormat.Flac,
                "wav" => GeneratedSpeechFormat.Wav,
                "pcm" => GeneratedSpeechFormat.Pcm,
                _ => GeneratedSpeechFormat.Mp3
            };
        }
    }
}
