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
    /// 语音转文字（Speech-To-Text）示例 - 使用 Whisper 模型
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
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//同步日志设置状态
        }

        public async Task RunAsync()
        {
            var sttSetting = Senparc.AI.Config.SenparcAiSetting;
            
            // 检查 API Key 配置
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
                await Console.Out.WriteLineAsync("Whisper 接口需要设置 OpenAI 或 AzureOpenAI ApiKey 后才能使用！");
                return;
            }

            await Console.Out.WriteLineAsync("========================================");
            await Console.Out.WriteLineAsync("Whisper 语音转文字（STT）示例开始运行");
            await Console.Out.WriteLineAsync("========================================");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("请输入音频文件路径（支持格式：flac, m4a, mp3, mp4, mpeg, mpga, oga, ogg, wav, webm）");
            await Console.Out.WriteLineAsync("输入 'exit' 退出程序");
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
                    await Console.Out.WriteLineAsync("请输入有效的文件路径！");
                    await Console.Out.WriteLineAsync();
                    continue;
                }

                // 检查文件是否存在
                if (!File.Exists(audioFilePath))
                {
                    await Console.Out.WriteLineAsync($"文件不存在：{audioFilePath}");
                    await Console.Out.WriteLineAsync();
                    continue;
                }

                try
                {
                    await Console.Out.WriteLineAsync("正在转换音频为文本，请等待...");
                    await Console.Out.WriteLineAsync();

                    // 读取音频文件
                    await using var audioFileStream = File.OpenRead(audioFilePath);
                    var audioFileBinaryData = await BinaryData.FromStreamAsync(audioFileStream);
                    var fileName = Path.GetFileName(audioFilePath);
                    var fileExtension = Path.GetExtension(audioFilePath).ToLower();

                    await Console.Out.WriteLineAsync($"[调试] 文件名: {fileName}, 扩展名: {fileExtension}");

                    // 创建音频内容（mimeType 设置为 null，让SDK自动处理）
                    var audioContent = new Microsoft.SemanticKernel.AudioContent(audioFileBinaryData, mimeType: null);
                    
                    // 创建执行设置，必须包含文件名
                    var executionSettings = new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIAudioToTextExecutionSettings(fileName)
                    {
                        Language = "zh" // 中文识别，如果是英文可以设置为 "en"
                    };

                    // 调用 Whisper API 进行语音转文字
                    var textContent = await audioToTextService.GetTextContentAsync(audioContent, executionSettings);

                    await Console.Out.WriteLineAsync("========================================");
                    await Console.Out.WriteLineAsync("转换结果：");
                    await Console.Out.WriteLineAsync("========================================");
                    await Console.Out.WriteLineAsync(textContent.Text);
                    await Console.Out.WriteLineAsync("========================================");
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
                await Console.Out.WriteLineAsync("请继续输入音频文件路径，或输入 'exit' 退出：");
            }

            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("STT 示例已退出。");
        }
    }
}
