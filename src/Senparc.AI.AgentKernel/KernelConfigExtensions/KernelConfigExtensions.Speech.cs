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
        /// 运行语音转文字（Speech-To-Text）并返回识别文本。
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="audioFilePath">音频文件路径。</param>
        /// <param name="options">识别选项。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>识别后的文本。</returns>
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
        /// 运行语音转文字（Speech-To-Text）并返回识别文本。
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="audioStream">音频流。</param>
        /// <param name="audioFileName">音频文件名（含扩展名）。</param>
        /// <param name="options">识别选项。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>识别后的文本。</returns>
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
        /// 运行文本转语音（Text-To-Speech）并返回音频二进制数据。
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="text">输入文本。</param>
        /// <param name="voice">音色（如 alloy / nova / echo）。</param>
        /// <param name="format">输出格式（如 mp3 / opus / wav）。</param>
        /// <param name="speedRatio">语速，推荐 0.5 - 2.0。</param>
        /// <param name="instructions">附加语音风格指令。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>音频二进制数据。</returns>
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
#pragma warning disable OPENAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            var options = new SpeechGenerationOptions
            {
                SpeedRatio = speedRatio,
                ResponseFormat = ParseGeneratedSpeechFormat(format),
                Instructions = instructions.IsNullOrEmpty() ? null : instructions
            };
#pragma warning restore OPENAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            var result = await iWantToRun.Kernel.TextToSpeechAsync(text, parsedVoice, options, cancellationToken);
            return result.Value;
        }

        /// <summary>
        /// 运行文本转语音（Text-To-Speech）并返回音频二进制数据。
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="text">输入文本。</param>
        /// <param name="voice">音色枚举。</param>
        /// <param name="options">生成选项。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>音频二进制数据。</returns>
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
        /// 将字符串音色转为 <see cref="GeneratedSpeechVoice"/>，无法识别时返回 Alloy。
        /// </summary>
        public static GeneratedSpeechVoice ParseGeneratedSpeechVoice(string voice)
        {
#pragma warning disable OPENAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
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
#pragma warning restore OPENAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        }

        /// <summary>
        /// 将字符串音频格式转为 <see cref="GeneratedSpeechFormat"/>，无法识别时返回 Mp3。
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
