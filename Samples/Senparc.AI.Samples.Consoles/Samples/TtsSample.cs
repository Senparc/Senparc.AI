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
    /// 文本转语音（Text-To-Speech）示例 - 使用 TTS 模型
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
            
            // 检查 API Key 配置
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
                await Console.Out.WriteLineAsync("TTS 接口需要设置 OpenAI 或 AzureOpenAI ApiKey 后才能使用！");
                return;
            }

            await Console.Out.WriteLineAsync("========================================");
            await Console.Out.WriteLineAsync("TTS 文本转语音（Text-To-Speech）示例开始运行");
            await Console.Out.WriteLineAsync("========================================");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("【声音选项】");
            await Console.Out.WriteLineAsync("  1. alloy   - 中性音色");
            await Console.Out.WriteLineAsync("  2. echo    - 男性音色");
            await Console.Out.WriteLineAsync("  3. fable   - 英式口音");
            await Console.Out.WriteLineAsync("  4. onyx    - 深沉男声");
            await Console.Out.WriteLineAsync("  5. nova    - 女性音色");
            await Console.Out.WriteLineAsync("  6. shimmer - 轻柔女声");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("【输出格式】");
            await Console.Out.WriteLineAsync("  mp3  - 最常用，兼容性好");
            await Console.Out.WriteLineAsync("  opus - 高压缩率，适合网络传输");
            await Console.Out.WriteLineAsync("  aac  - 适合流媒体");
            await Console.Out.WriteLineAsync("  flac - 无损音质");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("【语速范围】");
            await Console.Out.WriteLineAsync("  0.25x - 4.0x（默认 1.0x）");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("输入 'exit' 退出程序");
            await Console.Out.WriteLineAsync();

            var userId = "Jeffrey";
            var iWantTo = _semanticAiHandler.IWantTo(ttsSetting)
                                .ConfigModel(ConfigModel.TextToSpeech, userId)
                                .BuildKernel();

            var textToAudioService = iWantTo.GetRequiredService<ITextToAudioService>();

            string inputText;
            while (true)
            {
                await Console.Out.WriteLineAsync("请输入要转换为语音的文本（或输入 'exit' 退出）：");
                inputText = Console.ReadLine() ?? "";
                
                if (inputText.ToLower() == "exit")
                {
                    break;
                }

                if (inputText.IsNullOrEmpty())
                {
                    await Console.Out.WriteLineAsync("请输入有效的文本！");
                    await Console.Out.WriteLineAsync();
                    continue;
                }

                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("请选择声音（输入数字 1-6，默认为 1-alloy）：");
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
                await Console.Out.WriteLineAsync("请选择输出格式（输入数字 1-4，默认为 1-mp3）：");
                await Console.Out.WriteLineAsync("  1. mp3   - MP3 格式（推荐，兼容性好）");
                await Console.Out.WriteLineAsync("  2. opus  - Opus 格式（高压缩率）");
                await Console.Out.WriteLineAsync("  3. aac   - AAC 格式（适合流媒体）");
                await Console.Out.WriteLineAsync("  4. flac  - FLAC 格式（无损音质）");
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
                await Console.Out.WriteLineAsync("请选择语速（输入数字 1-7，默认为 4-正常速度 1.0x）：");
                await Console.Out.WriteLineAsync("  1. 0.25x - 极慢");
                await Console.Out.WriteLineAsync("  2. 0.5x  - 很慢");
                await Console.Out.WriteLineAsync("  3. 0.75x - 较慢");
                await Console.Out.WriteLineAsync("  4. 1.0x  - 正常（推荐）");
                await Console.Out.WriteLineAsync("  5. 1.25x - 较快");
                await Console.Out.WriteLineAsync("  6. 1.5x  - 很快");
                await Console.Out.WriteLineAsync("  7. 2.0x  - 极快");
                await Console.Out.WriteLineAsync("  8. 自定义（0.25 - 4.0）");
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
                    await Console.Out.WriteLineAsync($"正在生成语音（声音：{voice}，格式：{format}，语速：{speed}x），请等待...");
                    await Console.Out.WriteLineAsync();

                    // 创建执行设置
                    var executionSettings = new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAITextToAudioExecutionSettings
                    {
                        Voice = voice,           // 声音选项
                        ResponseFormat = format,  // 输出格式：mp3, opus, aac, flac
                        Speed = speed             // 语速：0.25 到 4.0
                    };

                    // 调用 TTS API 进行文本转语音
                    var audioContent = await textToAudioService.GetAudioContentAsync(inputText, executionSettings);

                    // 保存音频文件
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var outputFileName = $"TTS-Output-{voice}-{speed}x-{timestamp}.{format}";
                    var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);

                    await Console.Out.WriteLineAsync($"[调试] 音频数据大小: {audioContent.Data?.Length ?? 0} 字节");
                    await Console.Out.WriteLineAsync($"[调试] 参数 - 声音: {voice}, 格式: {format}, 语速: {speed}x");

                    if (audioContent.Data.HasValue && audioContent.Data.Value.Length > 0)
                    {
                        await File.WriteAllBytesAsync(outputFilePath, audioContent.Data.Value.ToArray());
                        
                        await Console.Out.WriteLineAsync("========================================");
                        await Console.Out.WriteLineAsync("语音生成成功！");
                        await Console.Out.WriteLineAsync($"声音：{voice}");
                        await Console.Out.WriteLineAsync($"格式：{format}");
                        await Console.Out.WriteLineAsync($"语速：{speed}x");
                        await Console.Out.WriteLineAsync($"保存路径：{outputFilePath}");
                        await Console.Out.WriteLineAsync($"文件大小：{new FileInfo(outputFilePath).Length} 字节");
                        await Console.Out.WriteLineAsync("========================================");
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("错误：未能获取音频数据");
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"转换失败：{ex.Message}");
                    if (ex.InnerException != null)
                    {
                        await Console.Out.WriteLineAsync($"详细错误：{ex.InnerException.Message}");
                    }
                }

                await Console.Out.WriteLineAsync();
            }

            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("TTS 示例已退出。");
        }

        /// <summary>
        /// 获取用户自定义的语速
        /// </summary>
        /// <returns></returns>
        private async Task<float> GetCustomSpeedAsync()
        {
            await Console.Out.WriteLineAsync("请输入自定义语速（0.25 - 4.0）：");
            var customSpeedInput = Console.ReadLine() ?? "1.0";
            
            if (float.TryParse(customSpeedInput, out var customSpeed))
            {
                // 限制范围在 0.25 到 4.0 之间
                if (customSpeed < 0.25f)
                {
                    await Console.Out.WriteLineAsync("语速过慢，已调整为最小值 0.25x");
                    return 0.25f;
                }
                else if (customSpeed > 4.0f)
                {
                    await Console.Out.WriteLineAsync("语速过快，已调整为最大值 4.0x");
                    return 4.0f;
                }
                return customSpeed;
            }
            else
            {
                await Console.Out.WriteLineAsync("输入无效，使用默认语速 1.0x");
                return 1.0f;
            }
        }
    }
}
