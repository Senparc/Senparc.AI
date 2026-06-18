using OpenAI.Audio;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Speech-To-Text 示例（Whisper）。
/// </summary>
public class SttSample
{
    private readonly IAiHandler _aiHandler;

    public SttSample(IAiHandler aiHandler)
    {
        _aiHandler = aiHandler;
        if (aiHandler is AgentAiHandler h)
        {
            h.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);
        }
    }

    public async Task RunAsync()
    {
        if (_aiHandler is not AgentAiHandler agentHandler)
        {
            throw new InvalidOperationException("当前示例需要 AgentAiHandler。");
        }

        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("STT（Speech-To-Text）示例");
        Console.WriteLine("支持输入格式：flac, m4a, mp3, mp4, mpeg, mpga, oga, ogg, wav, webm");
        Console.WriteLine("输入音频文件路径，输入 exit 退出。");
        Console.WriteLine("提示：可先尝试 `../../../STT-Test.m4a`。");
        Console.WriteLine();

        var iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigSpeechToTextModel("Jeffrey")
            .BuildKernel();

        while (true)
        {
            Console.WriteLine("音频文件路径：");
            var audioFilePath = Console.ReadLine();
            if (audioFilePath.IsNullOrEmpty())
            {
                continue;
            }

            if (audioFilePath.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (!File.Exists(audioFilePath))
            {
                Console.WriteLine("文件不存在，请重试。");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine("识别语言（可空，默认自动识别；例如 zh/en）：");
            var language = Console.ReadLine();

            try
            {
                var options = new AudioTranscriptionOptions
                {
                    Language = language.IsNullOrEmpty() ? null : language
                };
                var dt1 = SystemTime.Now;
                var result = await iWantToRun.Kernel.SpeechToTextAsync(audioFilePath, options);

                Console.WriteLine();
                Console.WriteLine($"识别结果(耗时：{SystemTime.DiffTotalMS(dt1)}ms)：");
                Console.WriteLine(result.Value.Text);
                Console.WriteLine($"[调试] Language={result.Value.Language}, Duration={result.Value.Duration}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("识别失败：" + ex.Message);
                Console.WriteLine();
            }
        }

        Console.WriteLine("STT 示例结束。");
    }
}
