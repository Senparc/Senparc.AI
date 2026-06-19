using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Single completion sample. See AgentAiHandlerTests.RunTest / SingleLineTest.
/// </summary>
public class CompletionSample
{
    private readonly IAiHandler _aiHandler;

    public CompletionSample(IAiHandler aiHandler)
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

        Console.WriteLine("CompletionSample started(No historical context. Each round is an independent request.)");
        Console.WriteLine("Enter exit to leave.");
        Console.WriteLine();

        var userId = "JeffreySu";
        var iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigModel(ConfigModel.TextCompletion, userId)
            .BuildKernel();

        while (true)
        {
            Console.WriteLine("Prompt:");
            var prompt = Console.ReadLine();
            if (prompt.IsNullOrEmpty())
            {
                Console.WriteLine("Please enter a prompt.");
                continue;
            }

            if (prompt.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine("Response:");
            try
            {
                var result = await iWantToRun.RunChatAsync(prompt);
                Console.WriteLine(result.Result.Text);
                Console.WriteLine($"[Debug] Tokens — total: {result.Result.Usage?.TotalTokenCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:" + ex);
            }

            Console.WriteLine();
        }
    }
}
