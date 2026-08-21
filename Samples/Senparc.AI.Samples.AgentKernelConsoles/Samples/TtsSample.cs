using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Text-to-speech sample.
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
            throw new InvalidOperationException("This sample requires AgentAiHandler.");
        }

        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("TTS (Text-to-Speech) sample");
        Console.WriteLine("Available voices: alloy / ash / ballad / coral / echo / fable / onyx / nova / sage / shimmer / verse");
        Console.WriteLine("Available formats: mp3 / opus / aac / flac / wav / pcm");
        Console.WriteLine("Recommended speed: 0.5 - 2.0, default 1.0");
        Console.WriteLine("Enter exit to leave.");
        Console.WriteLine();

        var iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigTextToSpeechModel("Jeffrey")
            .BuildKernel();

        while (true)
        {
            Console.WriteLine("Enter the text to synthesize:");
            var text = Console.ReadLine();
            if (text.IsNullOrEmpty())
            {
                continue;
            }

            if (text.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine("Voice (default alloy):");
            var voice = Console.ReadLine();
            voice = voice.IsNullOrEmpty() ? "alloy" : voice;

            Console.WriteLine("Format (default mp3):");
            var format = Console.ReadLine();
            format = format.IsNullOrEmpty() ? "mp3" : format;

            Console.WriteLine("Speed (default 1.0):");
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

                Console.WriteLine($"Generated successfully: {fileName}");
                Console.WriteLine($"Elapsed: {SystemTime.NowDiff(dt).TotalMilliseconds}ms");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generation failed: " + ex.Message);
                Console.WriteLine();
            }
        }

        Console.WriteLine("TTS sample ended.");
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
