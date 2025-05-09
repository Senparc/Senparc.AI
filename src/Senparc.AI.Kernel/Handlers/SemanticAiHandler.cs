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
    /// SenmanticKernel 处理器
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
        /// <param name="httpClient">为 null 时，自动使用 <see cref="LoggingHttpMessageHandler"/> 构建 <see cref="HttpClient" /></param>
        /// <param name="enableLog">是否开启 <paramref name="httpClient"/> 的日志（仅在 <paramref name="httpClient"/> 为 null 时，会自动构建 <see cref="LoggingHttpMessageHandler"/> 时生效。</param>
        public SemanticAiHandler(ISenparcAiSetting senparcAiSetting, SemanticKernelHelper? semanticAiHelper = null, ILoggerFactory loggerFactory = null, HttpClient httpClient = null, bool enableLog = false)
        {
            SemanticKernelHelper = semanticAiHelper ?? new SemanticKernelHelper(senparcAiSetting, loggerFactory, httpClient, enableLog);
            this.loggerFactory = loggerFactory;
        }

        /// <summary>
        /// <inheritdoc/>
        /// 未正式启用
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public SenparcAiResult Run(SenparcAiRequest request, ISenparcAiSetting? senparcAiSetting = null)
        {
            //TODO:未正式启用

            //TODO:此方法暂时还不能用

            var kernelBuilder = SemanticKernelHelper.ConfigTextCompletion(request.UserId, senparcAiSetting: senparcAiSetting);
            var kernel = kernelBuilder.Build();
            // KernelResult result = await kernel.RunAsync(input: request.RequestContent!, pipeline: request.FunctionPipeline);

            var result = new SenparcKernelAiResult(request.IWantToRun, request.RequestContent);
            return result;
        }

        /// <summary>
        /// 配置 Chat 参数
        /// </summary>
        /// <param name="promptConfigParameter"></param>
        /// <param name="userId"></param>
        /// <param name="modelName"></param>
        /// <param name="chatSystemMessage">System Message，仅在 <paramref name="promptTemplate"/> 为 null 时有效，否则会被忽略</param>
        /// <param name="promptTemplate">完整的 Prompt，一般会包含 System Message，设置后 <paramref name="chatSystemMessage"/> 参数会被忽略</param>
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

            //需要在 iWantToConfig.BuildKernel() 之前运行
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
        /// 获取聊天结果
        /// </summary>
        /// <param name="iWantToRun"></param>
        /// <param name="input">本次聊天内容</param>
        /// <param name="keepHistoryCount">需要保留的聊天记录条数（建议为 5-20 条）</param>
        /// <param name="inStreamItemProceessing">启用流，并指定遍历异步流每一步需要执行的委托。注意：只要此项不为 null，则会触发流式的请求。</param>
        /// <returns></returns>
        public async Task<SenparcAiResult> ChatAsync(IWantToRun iWantToRun, string input,
        Action<StreamingKernelContent> inStreamItemProceessing = null,
        string humanId = "User", string robotId = "Assistant", string historyArgName = "history", string humanInputArgName = "human_input",
        PromptConfigParameter? parameter = null)
        {
            //var function = iWantToRun.Kernel.Plugins.GetSemanticFunction("Chat");
            //request.FunctionPipeline = new[] { function };

            var request = iWantToRun.CreateRequest(true);

            //历史记录
            //初始化对话历史（可选）
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

            //运行

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

            //判断最大历史记录数
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;

            //清理对话历史记录条数
            if (chatHistory != null &&
                iWantTo.TempStore.TryGetValue("MaxHistoryCount", out object maxHistoryCountObj) &&
                (maxHistoryCountObj is int maxHistoryCount))
            {

                this.RemoveHistory(chatHistory, maxHistoryCount - 1);
            }

            aiResult.SetLastFunctionResultContent();
            //newHistory = newHistory + $"\n{humanId}: {input}\n{robotId}: {aiResult.OutputString}";
            chatHistory.AddAssistantMessage(aiResult.OutputString);

            //记录对话历史（可选）
            //request.SetStoredContext(historyArgName, newHistory);
            request.SetStoredContext(historyArgName, chatHistory);

            return aiResult;
        }

        /// <summary>
        /// 保留指定条数的历史记录
        /// </summary>
        /// <param name="history"></param>
        /// <param name="maxHistoryCount"></param>
        /// <returns></returns>
        public string RemoveHistory(string history, int maxHistoryCount, string humanId = "Human", string robotId = "ChatBot")
        {
            // 匹配 Human:xxx 和 Robot:xxx  
            string pattern = $@"{humanId}:.*?{robotId}:.*?(?=({humanId}:|$))";

            // 找到所有的匹配  
            MatchCollection matches = Regex.Matches(history, pattern, RegexOptions.Singleline);

            if (matches.Count > maxHistoryCount)
            {
                int removeCount = matches.Count - maxHistoryCount; // 指定要替换的匹配数量  
                int count = 0; // 已经替换的匹配数量  

                history = Regex.Replace(history, pattern, m => ++count <= removeCount ? "" : m.Value, RegexOptions.Singleline);
            }

            return history;
        }


        /// <summary>
        /// 保留指定条数的历史记录
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

                    //中间可能还有其他类型，或 tool

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