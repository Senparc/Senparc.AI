using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NetTopologySuite.Index.HPRtree;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Helpers;
using Senparc.AI.Kernel.HttpMessageHandlers;

namespace Senparc.AI.Kernel
{
    /// <summary>
    /// SemanticKernel handler
    /// </summary>
    public class SemanticAiHandler : IAiHandler<SenparcAiRequest, SenparcAiResult, SenparcAiArguments>
    {
        private readonly ILoggerFactory loggerFactory;

        public SemanticKernelHelper SemanticKernelHelper { get; set; }
        private Microsoft.SemanticKernel.Kernel _kernel => SemanticKernelHelper.GetKernel();

        /// <summary>
        ///
        /// </summary>
        /// <param name="senparcAiSetting"></param>
        /// <param name="semanticAiHelper"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="httpClient">When null, automatically build <see cref="HttpClient" /> with <see cref="LoggingHttpMessageHandler"/></param>
        /// <param name="enableLog">Whether to enable logging for <paramref name="httpClient"/>. Effective only when <paramref name="httpClient"/> is null and <see cref="LoggingHttpMessageHandler"/> is automatically built.</param>
        public SemanticAiHandler(ISenparcAiSetting senparcAiSetting, SemanticKernelHelper? semanticAiHelper = null, ILoggerFactory loggerFactory = null, HttpClient httpClient = null, bool enableLog = false)
        {
            SemanticKernelHelper = semanticAiHelper ?? new SemanticKernelHelper(senparcAiSetting, loggerFactory, httpClient, enableLog);
            this.loggerFactory = loggerFactory;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Not officially enabled
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public SenparcAiResult Run(SenparcAiRequest request, ISenparcAiSetting? senparcAiSetting = null)
        {
            //TODO:Not officially enabled

            //TODO:This method is not currently available

            var kernelBuilder = SemanticKernelHelper.ConfigTextCompletion(request.UserId, senparcAiSetting: senparcAiSetting);
            var kernel = kernelBuilder.Build();
            // KernelResult result = await kernel.RunAsync(input: request.RequestContent!, pipeline: request.FunctionPipeline);

            var result = new SenparcKernelAiResult(request.IWantToRun, request.RequestContent);
            return result;
        }

        /// <summary>
        /// Configure Chat parameters
        /// </summary>
        /// <param name="promptConfigParameter"></param>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="chatSystemMessage">System Message. Effective only when <paramref name="promptTemplate"/> is null; otherwise it is ignored</param>
        /// <param name="promptTemplate">Complete Prompt, usually including the System Message. After it is set, the <paramref name="chatSystemMessage"/> parameter is ignored</param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public IWantToRun ChatConfig(PromptConfigParameter promptConfigParameter,
            string userId,
            int maxHistoryStore,
            ModelName modelName = null,
            string chatSystemMessage = null,
            string promptTemplate = null,
            ISenparcAiSetting senparcAiSetting = null,
            Action<IKernelBuilder> kernelBuilderAction = null,
            string humanId = "User", string robotId = "Assistant", string hisgoryArgName = "history", string humanInputArgName = "human_input")
        {
            //promptTemplate ??= DefaultSetting.GetPromptForChat(chatSystemMessage ?? DefaultSetting.DEFAULT_SYSTEM_MESSAGE, humanId, robotId, hisgoryArgName, humanInputArgName);

            var iWantToConfig = this.IWantTo(senparcAiSetting)
                .ConfigModel(ConfigModel.Chat, userId, modelName);

            //Must run before iWantToConfig.BuildKernel()
            kernelBuilderAction?.Invoke(iWantToConfig.IWantTo.KernelBuilder);

            var iWanToRun = iWantToConfig.BuildKernel()
                .CreateFunctionFromPrompt(chatSystemMessage, promptConfigParameter)
                .iWantToRun;

            var iWantTo = iWantToConfig.IWantTo;
            iWantTo.TempStore["MaxHistoryCount"] = maxHistoryStore;

            var chatHistory = new ChatHistory();
            chatHistory.Add(new ChatMessageContent(AuthorRole.System, chatSystemMessage ?? DefaultSetting.DEFAULT_SYSTEM_MESSAGE));
            iWanToRun.StoredAiArguments.KernelArguments.Set(hisgoryArgName, chatHistory);

            return iWanToRun;
        }

        /// <summary>
        /// Get chat result
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="input">Current chat content</param>
        /// <param name="keepHistoryCount">Number of chat history records to keep. 5 to 20 is recommended.</param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        /// <returns></returns>
        public async Task<SenparcAiResult> ChatAsync(IWantToRun iWantToRun, string input,
        Action<StreamingKernelContent> inStreamItemProceessing = null,
        string humanId = "User", string robotId = "Assistant", string historyArgName = "history", string humanInputArgName = "human_input",
        PromptConfigParameter? parameter = null)
        {
            //var function = iWantToRun.Kernel.Plugins.GetSemanticFunction("Chat");
            //request.FunctionPipeline = new[] { function };

            var request = iWantToRun.CreateRequest(true);

            //History records
            //Initialize conversation history (optional)
            ChatHistory chatHistory;
            if (!request.GetStoredArguments(historyArgName, out var hiistoryObj))
            {
                //request.SetStoredContext(historyArgName, "");
                request.SetStoredContext(historyArgName, new ChatHistory());
                chatHistory = new ChatHistory();
                request.SetStoredContext(humanInputArgName, chatHistory);
            }
            else
            {
                chatHistory = hiistoryObj as ChatHistory;

            }

            var newRequest = request with { RequestContent = "" };

            //run

            SenparcKernelAiResult<string>? aiResult = null;
            List<IContentItem> visionResult = await ChatHelper.TryGetImagesBase64FromContent(Senparc.CO2NET.SenparcDI.GetServiceProvider(), input);
            aiResult = await iWantToRun.RunChatVisionAsync(newRequest, chatHistory, visionResult, parameter, inStreamItemProceessing);
            //            if (visionResult.Exists(z => z.Type == ContentType.Image))
            //            {
            //                aiResult = await iWantToRun.RunVisionAsync(newRequest, chatHistory, visionResult, inStreamItemProceessing);
            //            }
            //            else
            //            {
            //                aiResult = await iWantToRun.RunAsync(newRequest, inStreamItemProceessing);
            //            }

            //Check the maximum history record count
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            //Trim chat history record count
            if (chatHistory != null &&
                iWantTo.TempStore.TryGetValue("MaxHistoryCount", out object maxHistoryCountObj) &&
                (maxHistoryCountObj is int maxHistoryCount))
            {

                this.RemoveHistory(chatHistory, maxHistoryCount - 1);
            }

            aiResult.SetLastFunctionResultContent();
            //newHistory = newHistory + $"\n{humanId}: {input}\n{robotId}: {aiResult.OutputString}";
            chatHistory.AddAssistantMessage(aiResult.OutputString);

            //Record conversation history (optional)
            //request.SetStoredContext(historyArgName, newHistory);
            request.SetStoredContext(historyArgName, chatHistory);

            return aiResult;
        }

        /// <summary>
        /// Keep the specified number of history records
        /// </summary>
        /// <param name="history"></param>
        /// <param name="maxHistoryCount"></param>
        /// <returns></returns>
        public string RemoveHistory(string history, int maxHistoryCount, string humanId = "Human", string robotId = "ChatBot")
        {
            // Match Human:xxx and Robot:xxx
            string pattern = $@"{humanId}:.*?{robotId}:.*?(?=({humanId}:|$))";

            // Find all matches
            MatchCollection matches = Regex.Matches(history, pattern, RegexOptions.Singleline);

            if (matches.Count > maxHistoryCount)
            {
                int removeCount = matches.Count - maxHistoryCount; // Number of matches to replace
                int count = 0; // Number of matches already replaced

                history = Regex.Replace(history, pattern, m => ++count <= removeCount ? "" : m.Value, RegexOptions.Singleline);
            }

            return history;
        }


        /// <summary>
        /// Keep the specified number of history records
        /// </summary>
        /// <param name="chatHistory"></param>
        /// <param name="maxHistoryCount"></param>
        /// <returns></returns>
        public void RemoveHistory(ChatHistory chatHistory, int maxHistoryCount)
        {
            if (maxHistoryCount > 0)
            {
                var currentUserCount = chatHistory.Count(z => z.Role == AuthorRole.User);
                var removeCount = currentUserCount - maxHistoryCount;
                while (removeCount > 0)
                {
                    var firstUser = chatHistory.First(z => z.Role == AuthorRole.User);
                    var firstUserIndex = chatHistory.IndexOf(firstUser);

                    chatHistory.Remove(firstUser);

                    var removeList = chatHistory.Skip(firstUserIndex).TakeWhile(z => z.Role != AuthorRole.User).ToList();

                    //Other types or tools may appear in between

                    for (int i = 0; i < removeList.Count(); i++)
                    {
                        chatHistory.Remove(removeList[i]);
                    }

                    //var firstAssistant = chatHistory.FirstOrDefault(z => z.Role == AuthorRole.Assistant);
                    //if (firstAssistant != null)
                    //{
                    //    chatHistory.Remove(firstAssistant);
                    //}

                    removeCount--;
                }
            }
        }
    }
}