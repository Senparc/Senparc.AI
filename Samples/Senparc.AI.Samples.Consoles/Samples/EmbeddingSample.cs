using Azure;
using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using System.Reflection.Metadata;
using System.Text;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public enum ContentType
    {
        File,
        HtmlContent
    }

    public class EmbeddingSample
    {
        IAiHandler _aiHandler;

        SemanticAiHandler _semanticAiHandler => (SemanticAiHandler)_aiHandler;
        string _userId = "Jeffrey";
        string memoryCollectionName = "EmbeddingTest";
        string textEmbeddingGenerationName = "text-embedding-ada-002";
        string textEmbeddingAzureDeployName = "text-embedding-ada-002";

        public EmbeddingSample(IAiHandler aiHandler)
        {
            _aiHandler = aiHandler;
            _semanticAiHandler.SemanticKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);//同步日志设置状态
        }

        public async Task RunAsync(bool isReference = false, bool isRag = false)
        {
            if (isReference)
            {
                await Console.Out.WriteLineAsync("EmbeddingSample 开始运行。请输入需要 Embedding 的内容，id 和 text 以 :::（三个英文冒号）分割，输入 n 继续下一步。");
            }
            else
            {
                await Console.Out.WriteLineAsync("EmbeddingSample 开始运行。请输入需要 Embedding 的内容，URL 和介绍以 :::（三个英文冒号）分割，输入 n 继续下一步。");
            }
            await Console.Out.WriteLineAsync("请输入");

            //测试 TextEmbedding
            var iWantToRun = _semanticAiHandler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, _userId)
                 .ConfigModel(ConfigModel.TextCompletion, _userId)
                 .BuildKernel();


            //.BuildKernel(b => b.WithMemoryStorage(new VolatileMemoryStore()));

            //开始对话
            var i = 0;
            while (true)
            {
                var prompt = Console.ReadLine();
                if (prompt == "n")
                {
                    break;
                }

                var info = prompt.Split(new[] { ":::" }, StringSplitOptions.None);


                if (isReference)
                {
                    iWantToRun.MemorySaveReference(
                         modelName: textEmbeddingGenerationName,
                         azureDeployName: textEmbeddingAzureDeployName,
                         collection: memoryCollectionName,
                         description: info[1],//只用于展示记录
                         text: info[1],//真正用于生成 embedding
                         externalId: info[0],
                         externalSourceName: memoryCollectionName
                        );
                    await Console.Out.WriteLineAsync($"  URL {i + 1} saved");
                }
                else
                {
                    iWantToRun
                    .MemorySaveInformation(
                        modelName: textEmbeddingGenerationName,
                        azureDeployName: textEmbeddingAzureDeployName,
                        collection: memoryCollectionName, id: info[0], text: info[1]);
                }
                i++;
            }

            iWantToRun.MemoryStoreExexute();

            while (true)
            {
                await Console.Out.WriteLineAsync("请提问：");
                var question = Console.ReadLine();
                if (question == "exit")
                {
                    break;
                }

                var questionDt = DateTime.Now;
                var limit = isReference ? 3 : 1;
                var result = await iWantToRun.MemorySearchAsync(
                        modelName: textEmbeddingGenerationName,
                        azureDeployName: textEmbeddingAzureDeployName,
                        memoryCollectionName: memoryCollectionName,
                        query: question,
                        limit: limit);

                var j = 0;
                if (isReference)
                {
                    await foreach (var item in result.MemoryQueryResult)
                    {
                        await Console.Out.WriteLineAsync($"应答结果[{j + 1}]：");
                        await Console.Out.WriteLineAsync("  URL:\t\t" + item.Metadata.Id?.Trim());
                        await Console.Out.WriteLineAsync("  Description:\t" + item.Metadata.Description);
                        await Console.Out.WriteLineAsync("  Text:\t\t" + item.Metadata.Text);
                        await Console.Out.WriteLineAsync("  Relevance:\t" + item.Relevance);
                        await Console.Out.WriteLineAsync($"-- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                        j++;
                    }
                }
                else
                {
                    await foreach (var item in result.MemoryQueryResult)
                    {
                        var response = item;
                        if (response != null)
                        {
                            j++;
                        }
                        else
                        {
                            continue;
                        }

                        await Console.Out.WriteLineAsync($"应答[{j + 1}]： " + response.Metadata.Text +
                            $"\r\n -- Relevance {response.Relevance} -- cost {(DateTime.Now - questionDt).TotalMilliseconds}ms");
                    }
                }

                if (j == 0)
                {
                    await Console.Out.WriteLineAsync("无匹配结果");
                }

                await Console.Out.WriteLineAsync();

            }

        }

        public async Task RunRagAsync()
        {
            Console.WriteLine("请输入文件路径（.txt，.md 等文本文件），或文件目录（自动扫描其下所有 .txt 或 .md 文件），或 URL（自动下载网页内容），输入 end 停止输入，进入下一步");
          
            //RAG
            Dictionary<ContentType, string> contentMap = new Dictionary<ContentType, string>();
            //输入文件路径
            string filePath = "";
            while ((filePath = Console.ReadLine()) != "end")
            {
                if (Uri.TryCreate(filePath, UriKind.Absolute, out Uri? uriResult) 
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    Console.WriteLine("开始下载网页内容");
                    // 如果是URL，下载网页内容

                    var htmlContent = await Senparc.CO2NET.HttpUtility.RequestUtility.HttpGetAsync(Senparc.CO2NET.SenparcDI.GetServiceProvider(), filePath, Encoding.UTF8);
                    contentMap[ContentType.HtmlContent] = htmlContent;

                    // 去除 HTML 标签和不必要内容
                    htmlContent = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<script[^>]*>.*?</script>", "", System.Text.RegularExpressions.RegexOptions.Singleline);
                    htmlContent = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<style[^>]*>.*?</style>", "", System.Text.RegularExpressions.RegexOptions.Singleline);
                    htmlContent = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<!--.*?-->", "", System.Text.RegularExpressions.RegexOptions.Singleline);
                    htmlContent = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<[^>]+>", " ");
                    
                    // 清理多余空白字符
                    htmlContent = System.Text.RegularExpressions.Regex.Replace(htmlContent, @"\s+", " ").Trim();
                    
                    // 更新字典中的内容
                    contentMap[ContentType.HtmlContent] = htmlContent;
                    System.Console.WriteLine(htmlContent);
                    Console.WriteLine("下载网页内容成功");
                
                }
                else
                {
                    // 如果是普通文件路径
                    contentMap[ContentType.File] = filePath;
                }
                Console.WriteLine("请继续输入，直到输入 end 停止...");
            }


            //测试 TextEmbedding
            var iWantToRunEmbedding = _semanticAiHandler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, _userId)
                 .ConfigModel(ConfigModel.TextCompletion, _userId)
            .BuildKernel();

          
            contentMap.AsParallel().ForAll(async file =>
            {
                var text = file.Key == ContentType.File ? await File.ReadAllTextAsync(file.Value) : file.Value;
                List<string> paragraphs = new List<string>();

                if (file.Value.EndsWith(".md"))
                {
                    paragraphs = TextChunker.SplitMarkdownParagraphs(
                        TextChunker.SplitMarkDownLines(text.Replace("\r\n", " "), 128),
                        64);
                }
                else
                {
                    paragraphs = TextChunker.SplitPlainTextParagraphs(
                        TextChunker.SplitPlainTextLines(text.Replace("\r\n", " "), 128),
                        64);
                }


                var i = 0;
                paragraphs.ForEach(async paragraph =>
                {
                    iWantToRunEmbedding
                    .MemorySaveInformation(
                        modelName: textEmbeddingGenerationName,
                        azureDeployName: textEmbeddingAzureDeployName,
                        collection: memoryCollectionName,
                        id: $"paragraph{i++}",
                        text: paragraph);
                });
            });

            Console.WriteLine("请开始对话");

            string question = "";
            StringBuilder results = new StringBuilder();

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var systemMessage = @$"你是一位咨询机器人，你将根据我所提供的""提问""以及""备选信息""组织语言，生成一段给我的回复。
""备选信息""可能有多条，使用 ////// 表示每一条信息的开头，******表示每一天哦信息的结尾。在 ******后会有一个数字，表示这条信息的相关性。
你必须：
 - 将回答内容严格限制在我所提供给你的备选信息中（开头和结尾标记中间的内容），其中越靠前的备选信息可信度越高，相关性不属于答案内容本身，因此在组织语言的过程中必须将其忽略。
 - 请严格从""备选信息""中挑选和""提问""有关的信息，不要输出没有相关依据的信息。";

            var iWantToRunChat = _semanticAiHandler.ChatConfig(parameter,
                                 userId: "Jeffrey",
                                 maxHistoryStore: 10,
                                 chatSystemMessage: systemMessage,
                                 senparcAiSetting: null);
            while (true)
            {
                Console.WriteLine("提问：");
                question = Console.ReadLine();

                if (question.Trim().IsNullOrEmpty())
                {
                    Console.WriteLine("请输入有效内容");
                    continue;
                }else if(question== "exit")
                {
                    break;
                }
                
                var questionDt = DateTime.Now;
                var limit = 3;
                var embeddingResult = await iWantToRunEmbedding.MemorySearchAsync(
                        modelName: textEmbeddingGenerationName,
                        azureDeployName: textEmbeddingAzureDeployName,
                        memoryCollectionName: memoryCollectionName,
                        query: question,
                        limit: limit);

                await foreach (var item in embeddingResult.MemoryQueryResult)
                {
                    results.AppendLine($@"//////
{item.Metadata.Text}
******{item.Relevance}
");
                }

                SenparcTrace.SendCustomLog("RAG日志", $@"提问：{question}，耗时：{(DateTime.Now - questionDt).TotalMilliseconds}ms
结果：{results.ToString()}");

                Console.WriteLine();

                Console.Write("回答：");

                var input = @$"提问：{question}
备选答案：
{results.ToString()}";

                var useStream = iWantToRunChat.IWantToBuild.IWantToConfig.IWantTo.SenparcAiSetting.AiPlatform != AiPlatform.NeuCharAI;
                if (useStream)
                {
                    //使用流式输出

                    var originalColor = Console.ForegroundColor;//原始颜色
                    Action<StreamingKernelContent> streamItemProceessing = async item =>
                    {
                        await Console.Out.WriteAsync(item.ToString());

                        //每个流式输出改变一次颜色
                        if (Console.ForegroundColor == originalColor)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else
                        {
                            Console.ForegroundColor = originalColor;
                        }
                    };

                    //输出结果
                    SenparcAiResult result = await _semanticAiHandler.ChatAsync(iWantToRunChat, input, streamItemProceessing);

                    //复原颜色
                    Console.ForegroundColor = originalColor;
                }
                else
                {
                    //iWantToRunChat
                    var result = await _semanticAiHandler.ChatAsync(iWantToRunChat, input);
                    await Console.Out.WriteLineAsync(result.OutputString);
                }

                await Console.Out.WriteLineAsync();
            }
        }
    }
}
