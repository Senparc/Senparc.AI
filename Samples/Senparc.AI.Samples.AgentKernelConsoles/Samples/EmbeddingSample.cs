using Microsoft.Extensions.VectorData;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.KernelConfigExtensions;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// Embedding 生成与向量检索，参考 KernelConfigExtensions.EmbeddingTests.EmbeddingStoreTest。
/// </summary>
public class EmbeddingSample
{
    private readonly IAiHandler _aiHandler;
    private const string UserId = "Jeffrey";
    private const string CollectionName = "AgentKernelEmbeddingSample";

    public EmbeddingSample(IAiHandler aiHandler)
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

        var setting = SampleSetting.CurrentSetting;
        Console.WriteLine("EmbeddingSample：先生成向量并写入内存向量库，再进行相似度检索。");
        Console.WriteLine("录入阶段：每行输入一段文本，输入 n 进入检索阶段。");
        Console.WriteLine();

        var iWantToRun = agentHandler.IWantTo(setting)
            .ConfigTextEmbeddingModel(UserId, CollectionName)
            .BuildKernel();

        //var vectorStore = iWantToRun.GetVectorStore(setting.VectorDB);
        var store = iWantToRun.CreateTextSearchStore();

        var id = 1ul;
        while (true)
        {
            Console.WriteLine($"[{id}] 请输入文本（n 结束录入）：");
            var text = Console.ReadLine();
            if (text == "n")
            {
                break;
            }

            if (text.IsNullOrEmpty())
            {
                continue;
            }

            await store.UpsertDocumentsAsync([
                new TextSearchDocument
                {
                    SourceId = id,
                    SourceName = $"doc-{id}",
                    SourceLink = $"local://doc-{id}",
                    Text = text
                }
            ]);

            var vec = await iWantToRun.GetEmbeddingAsync(text);
            Console.WriteLine($"[调试] 已写入 doc-{id}，向量维度：{vec.Length}");
            id++;
        }

        Console.WriteLine();
        Console.WriteLine("检索阶段：输入问题，输入 exit 退出。");

        while (true)
        {
            Console.WriteLine("请提问：");
            var question = Console.ReadLine();
            if (question.IsNullOrEmpty())
            {
                continue;
            }

            if (question.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var dt = DateTime.Now;
            var hits = await store.SearchAsync(question, 3);
            var j = 0;
            foreach (var item in hits)
            {
                j++;
                Console.WriteLine($"应答结果[{j}]：");
                Console.WriteLine($"  Id: {item.SourceId}");
                Console.WriteLine($"  Name: {item.SourceName}");
                Console.WriteLine($"  Text: {item.Text}");
                Console.WriteLine();
            }

            if (j == 0)
            {
                Console.WriteLine("无匹配结果");
            }

            Console.WriteLine($"[调试] 检索耗时 {(DateTime.Now - dt).TotalMilliseconds}ms");
            Console.WriteLine();
        }
    }
}
