using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// 单次补全示例，参考 AgentAiHandlerTests.RunTest / SingleLineTest。
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
            throw new InvalidOperationException("当前示例需要 AgentAiHandler。");
        }

        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("CompletionSample 开始运行（无历史上下文，每轮独立请求）");
        Console.WriteLine("输入 exit 退出。");
        Console.WriteLine();

        var userId = "JeffreySu";
        var iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigModel(ConfigModel.TextCompletion, userId)
            .BuildKernel();

        while (true)
        {
            Console.WriteLine("提示词：");
            var prompt = Console.ReadLine();
            if (prompt.IsNullOrEmpty())
            {
                Console.WriteLine("请填写提示词！");
                continue;
            }

            if (prompt.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            Console.WriteLine("回复：");
            try
            {
                var result = await iWantToRun.RunAsync(prompt);
                Console.WriteLine(result.Result.Text);
                Console.WriteLine($"[调试] Tokens — total: {result.Result.Usage?.TotalTokenCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生错误：" + ex);
            }

            Console.WriteLine();
        }
    }
}
