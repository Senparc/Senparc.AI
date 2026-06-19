using OpenAI.Audio;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Speech-to-text sample using Whisper.
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
            throw new InvalidOperationException("This sample requires AgentAiHandler.");
        }

        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("STT (Speech-to-Text) sample");
        Console.WriteLine("Supported input formats: flac, m4a, mp3, mp4, mpeg, mpga, oga, ogg, wav, webm");
        Console.WriteLine("Enter an audio file path, or enter exit to leave.");
        Console.WriteLine("Tip: try `../../../STT-Test.m4a` first.");
        Console.WriteLine();

        var iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigSpeechToTextModel("Jeffrey")
            .BuildKernel();

        while (true)
        {
            Console.WriteLine("Audio file path:");
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
                Console.WriteLine("File does not exist. Please try again.");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine("Recognition language (optional; empty uses automatic detection, for example zh/en):");
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
                Console.WriteLine($"Recognition result (elapsed: {SystemTime.DiffTotalMS(dt1)}ms):");
                Console.WriteLine(result.Value.Text);
                Console.WriteLine($"[Debug] Language={result.Value.Language}, Duration={result.Value.Duration}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Recognition failed: " + ex.Message);
                Console.WriteLine();
            }
        }

        Console.WriteLine("STT sample ended.");
    }
}
