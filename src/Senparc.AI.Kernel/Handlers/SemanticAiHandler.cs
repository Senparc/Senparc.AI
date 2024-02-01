using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

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


        public SemanticAiHandler(ISenparcAiSetting senparcAiSetting, SemanticKernelHelper? semanticAiHelper = null, ILoggerFactory loggerFactory = null)
        {
            SemanticKernelHelper = semanticAiHelper ?? new SemanticKernelHelper(senparcAiSetting);
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

            var kernelBuilder = SemanticKernelHelper.ConfigTextCompletion(request.UserId,senparcAiSetting: senparcAiSetting);
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
        /// <param name="chatPrompt"></param>
        /// <param name="senparcAiSetting"></param>
        /// <returns></returns>
        public (IWantToRun iWantToRun, KernelFunction chatFunction) ChatConfig(PromptConfigParameter promptConfigParameter,
            string userId,
            ModelName modelName = null,
            int maxHistoryStore = 0,
            string chatPrompt = Senparc.AI.DefaultSetting.DEFAULT_PROMPT_FOR_CHAT,
            ISenparcAiSetting senparcAiSetting = null)
        {
            var result = this.IWantTo(senparcAiSetting)
                .ConfigModel(ConfigModel.TextCompletion, userId, modelName)
                .BuildKernel()
                .CreateFunctionFromPrompt(chatPrompt, promptConfigParameter);

            var iWantTo = result.iWantToRun.IWantToBuild.IWantToConfig.IWantTo;
            iWantTo.TempStore["MaxHistoryCount"] = maxHistoryStore;

            return result;
        }

        public async Task<SenparcAiResult> ChatAsync(IWantToRun iWantToRun, string prompt)
        {
            //var function = iWantToRun.Kernel.Plugins.GetSemanticFunction("Chat");
            //request.FunctionPipeline = new[] { function };

            var request = iWantToRun.CreateRequest(true);

            //历史记录
            //初始化对话历史（可选）
            if (!request.GetStoredArguments("history", out var history))
            {
                request.SetStoredContext("history", "");
            }

            //本次记录
            request.SetStoredContext("human_input", prompt);

            var newRequest = request with { RequestContent = "" };

            //运行
            var aiResult = await iWantToRun.RunAsync(newRequest);

            string newHistory = history + $"\nHuman: {prompt}\nBot: {aiResult.Output}";

            //判断最大历史记录数
            var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;
            if (iWantTo.TempStore.TryGetValue("MaxHistoryCount", out object maxHistoryCountObj) &&
                (maxHistoryCountObj is int maxHistoryCount))
            {

            }

            //记录对话历史（可选）
            request.SetStoredContext("history", newHistory);


            return aiResult;
        }
    }
}