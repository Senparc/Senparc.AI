using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Text;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
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
using static Qdrant.Client.Grpc.MaxOptimizationThreads.Types;

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
        /// Use RAG question answering
        /// </summary>
        /// <returns></returns>
        public async Task RunRagAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Enter a file path (.txt, .md, or another text file), a directory path (automatically scans all .txt or .md files under it), or a URL (automatically downloads web content). Enter end to stop and continue to the next step.");

            //RAG
            List<KeyValuePair<ContentType, string>> contentMap = new List<KeyValuePair<ContentType, string>>();
            //Input file path
            string filePath = Console.ReadLine();
            while (filePath != "end")
            {
                //            if (Uri.TryCreate(filePath, UriKind.Absolute, out Uri? uriResult)
                //&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                if (filePath.ToUpper().StartsWith("HTTP://") || filePath.ToUpper().StartsWith("HTTPS://"))
                {
                    Console.WriteLine("Start fetching web content");
                    // If this is a URL, download web content

                    // Check whether the URL has depth and count limits
                    int depth = 1;
                    int maxCount = 1;
                    var match = Regex.Match(filePath, @">{0,}(\d*)$");
                    if (match.Success)
                    {
                        depth = Math.Max(1, match.Value.Count(c => c == '>')); // Use the number of > characters as depth
                        if (!int.TryParse(match.Groups[1].Value, out maxCount)) // Use the number as the maximum count
                        {
                            maxCount = 1;
                        }
                        // Remove depth and count markers from the URL
                        filePath = filePath.Substring(0, filePath.Length - match.Value.Length);
                    }
                    Console.WriteLine($"Set crawl depth:{depth}, maximum crawl count:{maxCount}");
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

                        var logStr = $"Download web content{(requestSuccess ? "succeeded" : "failed")}." +
                            (requestSuccess ? $"character count:{urlData.MarkDownHtmlContent?.Length}" : $"error code:{item.Value.Result}") +
                            $"\t URL:{item.Key.ToLower()}";

                        if (!requestSuccess)
                        {
                            logStr += $" source:{urlData.ParentUrl} (link:{urlData.LinkText})";
                        }

                        SenparcTrace.SendCustomLog("RAG log", logStr);
                        Console.WriteLine(logStr);
                    }
                }
                else
                {
                    // If this is a normal file path
                    contentMap.Add(new KeyValuePair<ContentType, string>(ContentType.File, filePath));
                }
                Console.WriteLine("Continue entering values until end is entered...");
                filePath = Console.ReadLine();
            }

            Console.WriteLine("Processing information...");

            //Create handler
            var embeddingAiSetting = ((SenparcAiSetting)SampleSetting.CurrentSetting) with { AiPlatform = AiPlatform.AzureOpenAI };
            var embeddingAiHandler = new SemanticAiHandler(embeddingAiSetting);

            //Test TextEmbedding
            var iWantToRunEmbedding = embeddingAiHandler
                 .IWantTo()
                 .ConfigModel(ConfigModel.TextEmbedding, _userId)
                 .ConfigVectorStore(embeddingAiSetting.VectorDB)
            .BuildKernel();

            var modelName = textEmbeddingGenerationName(embeddingAiSetting);

            var vectorName = "senparc-sample-rag";
            var vectorCollection = iWantToRunEmbedding.GetVectorCollection<ulong, Record>(embeddingAiSetting.VectorDB, vectorName);
            await vectorCollection.EnsureCollectionExistsAsync();

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
                       paragraphs.ForEach(async paragraph =>
                       {
                           var currentI = i++;

                           var record = new Record()
                           {
                               Id = (ulong)i,
                               Name = vectorName + "-paragraph-" + i,
                               Description = paragraph,
                               DescriptionEmbedding = await iWantToRunEmbedding.SemanticKernelHelper.GetEmbeddingAsync(modelName, paragraph),
                               Tags = new[] { i.ToString() }
                           };
                           await vectorCollection.UpsertAsync(record);
                       });
                   }
                   catch (Exception ex)
                   {
                       string pattern = @"retry after (\d+) seconds";
                       Match match = Regex.Match(ex.Message, pattern);
                       if (match.Success)
                       {
                           Console.WriteLine($"Waiting for cooldown {match.Value} seconds");
                       }
                       goto MemoryStore;
                   }

               });

            Console.WriteLine($"Processing completed(file count:{contentMap.Count}, paragraph count:{i})");

            Console.WriteLine("Start the conversation");

            string question = "";
            StringBuilder results = new StringBuilder();

            var parameter = new PromptConfigParameter()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5,
            };

            var systemMessage = @$"## SystemMessage
You are a consulting assistant. Use the Question and candidate information I provide to compose a Response.
Note: candidate information comes from Embedding search results related to the Question, so there may be multiple entries. Each candidate entry starts with ////// and ends with ******. A number after ****** indicates relevance to the Question. Only the content between ////// and ****** is useful for reasoning.

## Guidelines
You must:
 - Strictly limit the answer to the candidate information I provide, specifically the content between the start and end markers. Earlier candidate information is more credible. Relevance scores are not part of the answer and must be ignored when composing the response.
 - Strictly select information related to the question from the candidate information. Do not output unsupported information, and do not generate nonexistent information.
 - The information you receive may include our conversation history. You may refer to it, then answer based on my latest Question and the candidate information.";

            var iWantToRunChat = _semanticAiHandler.ChatConfig(parameter,
                                 userId: "Jeffrey",
                                 maxHistoryStore: 10,
                                 chatSystemMessage: systemMessage,
                                 senparcAiSetting: null);
            while (true)
            {
                Console.WriteLine("Question:");
                question = Console.ReadLine();

                if (question.Trim().IsNullOrEmpty())
                {
                    Console.WriteLine("Enter valid content");
                    continue;
                }
                else if (question == "exit")
                {
                    break;
                }

                var questionDt = DateTime.Now;
                var limit = 3;


                ReadOnlyMemory<float> searchVector = await iWantToRunEmbedding.SemanticKernelHelper.GetEmbeddingAsync(modelName, question);

                var vectorResult = vectorCollection.SearchAsync(searchVector, limit);


                await foreach (var item in vectorResult)
                {
                    results.AppendLine($@"//////
{item.Record.Description}
******{item.Score}");
                }

                SenparcTrace.SendCustomLog("RAG log", $@"Question:{question}, Elapsed time:{(DateTime.Now - questionDt).TotalMilliseconds}ms
Result:
{results.ToString()}
");

                Console.WriteLine();

                Console.Write("Answer:");

                var input = @$"### Question:{question}
### Candidate answers:
{results.ToString()}";

                var useStream = iWantToRunChat.IWantToBuild.IWantToConfig.IWantTo.SenparcAiSetting.AiPlatform != AiPlatform.NeuCharAI;
                if (useStream)
                {
                    //Use streaming output

                    var originalColor = Console.ForegroundColor;//Original color
                    Action<StreamingKernelContent> streamItemProceessing = async item =>
                    {
                        await Console.Out.WriteAsync(item.ToString());

                        //Change color for each streamed output
                        if (Console.ForegroundColor == originalColor)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else
                        {
                            Console.ForegroundColor = originalColor;
                        }
                    };

                    //Output result
                    SenparcAiResult result = await _semanticAiHandler.ChatAsync(iWantToRunChat, input, streamItemProceessing);

                    //Restore color
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
