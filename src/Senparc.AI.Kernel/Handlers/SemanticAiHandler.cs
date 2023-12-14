using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Senparc.AI.Entities;
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


        public SemanticAiHandler(SemanticKernelHelper? semanticAiHelper = null, ILoggerFactory loggerFactory = null)
        {
            SemanticKernelHelper = semanticAiHelper ?? new SemanticKernelHelper();
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
            var kernelBuilder = SemanticKernelHelper.ConfigTextCompletion(request.UserId, request.ModelName, senparcAiSetting, null,request.ModelName);
            var kernel = kernelBuilder.Build();
            // KernelResult result = await kernel.RunAsync(input: request.RequestContent!, pipeline: request.FunctionPipeline);

            var result = new SenparcKernelAiResult(request.IWantToRun, request.RequestContent);
            return result;
        }

        public (IWantToRun iWantToRun, KernelFunction chatFunction) ChatConfig(PromptConfigParameter promptConfigParameter,
            string userId, string modelName = "text-davinci-003")
        {
            var result = this.IWantTo()
                .ConfigModel(ConfigModel.TextCompletion, userId, modelName)
                .BuildKernel()
                .CreateFunctionFromPrompt("ChatBot", "Chat", promptConfigParameter);

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

            var newRequest = request with { RequestContent = null };

            //运行
            var aiResult = await iWantToRun.RunAsync(newRequest);

            //记录对话历史（可选）
            request.SetStoredContext("history", history + $"\nHuman: {prompt}\nBot: {aiResult.Output}");

            return aiResult;
        }
    }
}