using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
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
            string humanId = "User", string robotId = "Assistant", string hisgoryArgName = "history", string humanInputArgName = "human_input")
        {
            //promptTemplate ??= DefaultSetting.GetPromptForChat(chatSystemMessage ?? DefaultSetting.DEFAULT_SYSTEM_MESSAGE, humanId, robotId, hisgoryArgName, humanInputArgName);

            var iWanToRun = this.IWantTo(senparcAiSetting)
                .ConfigModel(ConfigModel.Chat, userId, modelName)
                .BuildKernel()
                .CreateFunctionFromPrompt(chatSystemMessage, promptConfigParameter)
                .iWantToRun;

            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
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
        string humanId = "User", string robotId = "Assistant", string historyArgName = "history", string humanInputArgName = "human_input")
        {
            //var function = iWantToRun.Kernel.Plugins.GetSemanticFunction("Chat");
            //request.FunctionPipeline = new[] { function };

            var request = iWantToRun.CreateRequest(true);

            //历史记录
            //初始化对话历史（可选）
            ChatHistory oldHistory;
            if (!request.GetStoredArguments(historyArgName, out var hiistoryObj))
            {
                //request.SetStoredContext(historyArgName, "");
                request.SetStoredContext(historyArgName, new ChatHistory());
                oldHistory = new ChatHistory();
            }
            else
            {
                 oldHistory = hiistoryObj as ChatHistory;
             
            }

            oldHistory.Add(new ChatMessageContent(AuthorRole.User, input));
            oldHistory.Add(new ChatMessageContent(AuthorRole.Assistant, ""));

            //本次记录
            request.SetStoredContext(humanInputArgName, oldHistory);

            var newRequest = request with { RequestContent = "" };


            //运行

            SenparcKernelAiResult<string>? aiResult = null;
            List<ContentItem> visionResult = await ChatHelper.TryGetImagesBase64FromContent(Senparc.CO2NET.SenparcDI.GetServiceProvider(), input);
            if (visionResult.Exists(z => z.Type == ContentType.Image))
            {
                aiResult = await iWantToRun.RunVisionAsync(newRequest, visionResult, inStreamItemProceessing);
            }
            else
            {
                aiResult = await iWantToRun.RunAsync(newRequest, inStreamItemProceessing);
            }

            //判断最大历史记录数
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;
            //string newHistory = null;
            ChatHistory newchatHistory = new ChatHistory();

            //if ((historyObj is string history) &&
            //    history != null &&
            //    iWantTo.TempStore.TryGetValue("MaxHistoryCount", out object maxHistoryCountObj) &&
            //    (maxHistoryCountObj is int maxHistoryCount))
            //{
            //    newHistory = this.RemoveHistory(history, maxHistoryCount - 1);
            //}
            //else
            //if (historyObj is ChatHistory)
            //{
            //    //TODO: 删除记录
            //    newchatHistory = historyObj as ChatHistory;
            //    //chatHistory.Add(new ChatMessageContent(AuthorRole.))
            //}

            //newHistory = newHistory + $"\n{humanId}: {input}\n{robotId}: {aiResult.OutputString}";
            oldHistory.Remove(oldHistory.Last());
            oldHistory.Add(new ChatMessageContent(AuthorRole.Assistant, aiResult.OutputString));

            //记录对话历史（可选）
            //request.SetStoredContext(historyArgName, newHistory);
            request.SetStoredContext(historyArgName, newchatHistory);

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
    }
}