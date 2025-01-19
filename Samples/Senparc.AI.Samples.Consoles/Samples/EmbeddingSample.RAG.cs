using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Text;
using Senparc.AI.Entities;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Trace;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using Senparc.Xncf.SenMapic.Domain.SiteMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Senparc.AI.Samples.Consoles.Samples
{
    public enum ContentType
    {
        File,
        HtmlContent
    }


    public partial class EmbeddingSample
    {

        /// <summary>
        /// 使用 RAG 问答
        /// </summary>
        /// <returns></returns>
        public async Task RunRagAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine("请输入文件路径（.txt，.md 等文本文件），或文件目录（自动扫描其下所有 .txt 或 .md 文件），或 URL（自动下载网页内容），输入 end 停止输入，进入下一步");

            //RAG
            List<KeyValuePair<ContentType, string>> contentMap = new List<KeyValuePair<ContentType, string>>();
            //输入文件路径
            string filePath = "";
            while (filePath != "end")
            {
                if (Uri.TryCreate(filePath, UriKind.Absolute, out Uri? uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    Console.WriteLine("开始下载网页内容");
                    // 如果是URL，下载网页内容

                    // 检查URL是否有深度和数量限制
                    int depth = 1;
                    int maxCount = 1;
                    var match = System.Text.RegularExpressions.Regex.Match(filePath, @">{1,}(\d+)$");
                    if (match.Success)
                    {
                        depth = match.Value.Count(c => c == '>'); // 获取>的数量作为深度
                        maxCount = int.Parse(match.Groups[1].Value); // 获取数字作为最大数量
                        // 移除URL中的深度和数量标记
                        filePath = filePath.Substring(0, filePath.Length - match.Value.Length);
                    }
                    Console.WriteLine($"设置抓取深度：{depth}，最大抓取数量：{maxCount}");
                    var engine = new SenMapicEngine(
                                serviceProvider: serviceProvider,
                                urls: new[] { filePath },
                                maxThread: 20,
                                maxBuildMinutesForSingleSite: 5,
                                maxDeep: depth,
                                maxPageCount: maxCount
                            );
                    
                    var senMapicResult = engine.Build();


                    foreach (var item in senMapicResult)
                    {
                        var urlData = item.Value;
                        var rawText = System.Text.RegularExpressions.Regex.Replace(urlData.MarkDownHtmlContent ?? "", @"[#*`_~\[\]()]+|\s+", " ").Trim();
                        contentMap.Add(new KeyValuePair<ContentType, string>(ContentType.HtmlContent, rawText));

                        var requestSuccess = urlData.Result == 200;

                        var logStr = $"下载网页内容{(requestSuccess ? "成功" : "失败")}。" +
                            (requestSuccess ? $"字符数：{urlData.MarkDownHtmlContent?.Length}" : $"错误代码：{item.Value.Result}") +
                            $"\t URL:{item.Key.ToLower()}";

                        if (!requestSuccess)
                        {
                            logStr += $" 来源：{urlData.ParentUrl} （链接：{urlData.LinkText}）";
                        }

                        SenparcTrace.SendCustomLog("RAG日志", logStr);
                        Console.WriteLine(logStr);
                    }
                }
                else
                {
                    // 如果是普通文件路径
                    contentMap.Add(new KeyValuePair<ContentType, string>(ContentType.File, filePath));
                }
                Console.WriteLine("请继续输入，直到输入 end 停止...");
                filePath = Console.ReadLine();
            }

            Console.WriteLine("正在处理信息...");

            //测试 TextEmbedding
            var iWantToRunEmbedding = _semanticAiHandler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, _userId)
                 .BuildKernel();

            var i = 0;
            var dt = SystemTime.Now;
            var mapTasks = new List<Task>();

            contentMap.ForEach(file =>
               {
                   if (file.Value.IsNullOrEmpty())
                   {
                       return;
                   }

                   var text = file.Key == ContentType.File ? File.ReadAllTextAsync(file.Value).Result : file.Value;
                   List<string> paragraphs = new List<string>();

                   if (file.Value.EndsWith(".md"))
                   {
                       paragraphs = TextChunker.SplitMarkdownParagraphs(
                         TextChunker.SplitMarkDownLines(System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Replace("\r\n", " "), 128),
                         64);
                   }
                   else
                   {
                       paragraphs = TextChunker.SplitPlainTextParagraphs(
                         TextChunker.SplitPlainTextLines(System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Replace("\r\n", " "), 128),
                         256);
                   }

               MemoryStore:
                   try
                   {
                       paragraphs.ForEach(paragraph =>
                       {
                           var currentI = i++;

                           iWantToRunEmbedding
                             .MemorySaveInformation(
                                 modelName: textEmbeddingGenerationName,
                                 azureDeployName: textEmbeddingAzureDeployName,
                                 collection: memoryCollectionName,
                                 id: $"paragraph-{Guid.NewGuid().ToString("n")}",
                                 text: paragraph);
                       });
                   }
                   catch (Exception ex)
                   {
                       string pattern = @"retry after (\d+) seconds";
                       Match match = Regex.Match(ex.Message, pattern);
                       if (match.Success)
                       {
                           Console.WriteLine($"等待冷却 {match.Value} 秒");
                       }
                       goto MemoryStore;
                   }
                   
               });

            Console.WriteLine($"处理完成(文件数：{contentMap.Count}，段落数：{i})");

            Console.WriteLine("请开始对话");

            string question = "";
            StringBuilder results = new StringBuilder();

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var systemMessage = @$"## SystemMessage
你是一位咨询机器人，你将根据我所提供的“提问”以及“备选信息”组织语言，生成一段给我的回复。
""备选信息""可能有多条，使用 ////// 表示每一条信息的开头， 表示每一条信息的结尾。在 ****** 后会有一个数字，表示这条信息的相关性。

## Rule
你必须：
 - 将回答内容严格限制在我所提供给你的备选信息中（开头和结尾标记中间的内容），其中越靠前的备选信息可信度越高，相关性不属于答案内容本身，因此在组织语言的过程中必须将其忽略。
 - 请严格从“备选信息”中挑选和“提问”有关的信息，不要输出没有相关依据的信息。";

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
                }
                else if (question == "exit")
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
******{item.Relevance}");
                }

                SenparcTrace.SendCustomLog("RAG日志", $@"提问：{question}，耗时：{(DateTime.Now - questionDt).TotalMilliseconds}ms
结果：
{results.ToString()}
");

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
