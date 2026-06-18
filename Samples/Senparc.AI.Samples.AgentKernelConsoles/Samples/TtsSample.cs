using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Text-To-Speech 示例。
/// </summary>
public class TtsSample
{
    private readonly IAiHandler _aiHandler;

    public TtsSample(IAiHandler aiHandler)
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

        Console.WriteLine("TTS（Text-To-Speech）示例");
        Console.WriteLine("可选音色：alloy / ash / ballad / coral / echo / fable / onyx / nova / sage / shimmer / verse");
        Console.WriteLine("可选格式：mp3 / opus / aac / flac / wav / pcm");
        Console.WriteLine("语速建议：0.5 - 2.0，默认 1.0");
        Console.WriteLine("输入 exit 退出。");
        Console.WriteLine();

        var iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigTextToSpeechModel("Jeffrey")
            .BuildKernel();

        while (true)
        {
            Console.WriteLine("请输入要合成的文本：");
            var text = Console.ReadLine();
            if (text.IsNullOrEmpty())
            {
                continue;
            }

            if (text.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine("音色（默认 alloy）：");
            var voice = Console.ReadLine();
            voice = voice.IsNullOrEmpty() ? "alloy" : voice;

            Console.WriteLine("格式（默认 mp3）：");
            var format = Console.ReadLine();
            format = format.IsNullOrEmpty() ? "mp3" : format;

            Console.WriteLine("语速（默认 1.0）：");
            var speedRaw = Console.ReadLine();
            var speed = 1.0f;
            if (!speedRaw.IsNullOrEmpty() && float.TryParse(speedRaw, out var parsedSpeed))
            {
                speed = Math.Clamp(parsedSpeed, 0.5f, 2.0f);
            }

            try
            {
                var dt = SystemTime.Now;
                var audio = await iWantToRun.RunTextToSpeechAsync(text, voice, format, speedRatio: speed);

                var normalizedFormat = NormalizeFormat(format);
                var fileName = $"TTS-{voice}-{SystemTime.NowTicks}.{normalizedFormat}";
                await File.WriteAllBytesAsync(fileName, audio.ToArray());

                Console.WriteLine($"生成成功：{fileName}");
                Console.WriteLine($"耗时：{SystemTime.NowDiff(dt).TotalMilliseconds}ms");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("生成失败：" + ex.Message);
                Console.WriteLine();
            }
        }

        Console.WriteLine("TTS 示例结束。");
    }

    private static string NormalizeFormat(string format)
    {
        return format?.Trim().ToLowerInvariant() switch
        {
            "mp3" => "mp3",
            "opus" => "opus",
            "aac" => "aac",
            "flac" => "flac",
            "wav" => "wav",
            "pcm" => "pcm",
            _ => "mp3"
        };
    }
}
