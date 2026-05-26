using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Exceptions;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Handlers
{
    public partial class KernelConfigExtensions
    {

        #region 运行

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunChatAsync(this IWantToRun iWanToRun, string prompt, AgentSession agentSession = null, Action<AgentResponseUpdate> inStreamItemProceessing = null)
        {
            return RunChatAsync<string>(iWanToRun, prompt, agentSession, inStreamItemProceessing);
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<T>> RunChatAsync<T>(this IWantToRun iWanToRun, string prompt, AgentSession agentSession = null, Action<AgentResponseUpdate> inStreamItemProceessing = null)
            where T : class
        {
            var request = iWanToRun.CreateRequest(prompt, agentSession);
            return RunChatAsync<T>(iWanToRun, request, inStreamItemProceessing);
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">启用流，并指定遍历异步流每一步需要执行的委托。注意：只要此项不为 null，则会触发流式的请求。</param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunChatAsync(this IWantToRun iWanToRun, SenparcAiRequest request, Action<AgentResponseUpdate> inStreamItemProceessing = null)
        {
            return RunChatAsync<string>(iWanToRun, request, inStreamItemProceessing);
        }

        /// <summary>
        /// 运行，兼容 Streamming
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">启用流，并指定遍历异步流每一步需要执行的委托。注意：只要此项不为 null，则会触发流式的请求。</param>
        /// <typeparam name="T">指定返回结果类型</typeparam>
        /// <returns></returns>

        public static async Task<SenparcKernelAiResult<T>> RunChatAsync<T>(this IWantToRun iWanToRun, SenparcAiRequest request, Action<AgentResponseUpdate> inStreamItemProceessing = null)
            where T : class
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.SemanticKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            var prompt = request.RequestContent;
            var session = request.AgentSession;
            var functionPipline = request.FunctionPipeline;
            //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

            //注意：只要使用了 Plugin 和 Function，并且包含输入标识，就需要使用上下文

            iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
            var storedArguments = iWanToRun.StoredAiArguments.KernelArguments;
            var tempArguments = request.TempAiArguments?.KernelArguments;

            AgentResponse agentResponse = null;
            var result = new SenparcKernelAiResult<T>(iWanToRun, inputContent: null);

            var useStream = inStreamItemProceessing != null;

            if (!useStream)
            {
                try
                {
                    agentResponse = await iWanToRun.Kernel.InvokeChatAsync(prompt, session);

                    if (typeof(T) == typeof(string))
                    {
                        result.OutputString = agentResponse.Text;//.GetValue<string>()?.TrimStart('\n') ?? "";
                    }
                    else
                    {
                        result.OutputString = agentResponse.RawRepresentation?.ToJson();//.GetValue<T>()?.ToJson()?.TrimStart('\n') ?? "";
                    }
                }
                catch (Exception ex)
                {
                    /* OpenAI 使用 JSON 格式可能出现异常：
                    Invalid parameter: 'response_format' of type 'json_schema' is not supported with this model
                    */

                    result.OutputString = ex.Message;
                    //TODO: 提供 Output 的泛型
                    //result.OutputString = agentResponse.RawRepresentation?.ToJson()?.TrimStart('\n') ?? "";
                    _ = new SenparcAiException("无法转换为指定类型：" + typeof(T).Name);
                }
                result.Result = agentResponse;
            }
            else
            {
                var stringResult = new StringBuilder();

                result.StreamResult = kernel.InvokeChatStreamingAsync(prompt, session);
                UsageContent usageContent = null;

                if (result.StreamResult != null)
                {
                    await foreach (var item in result.StreamResult)
                    {
                        stringResult.Append(item);
                        inStreamItemProceessing?.Invoke(item);//执行流

                        try
                        {
                            if (item.Contents?.FirstOrDefault(z => z is Microsoft.Extensions.AI.UsageContent)
                                is Microsoft.Extensions.AI.UsageContent usage)
                            {
                                usageContent = usage;
                            }
                        }
                        catch (Exception ex)
                        {
                            SenparcTrace.BaseExceptionLog(ex);
                        }
                    }
                }

                result.OutputString = stringResult.ToString();

                List<ChatMessage> history = new List<ChatMessage>();
                session?.TryGetInMemoryChatHistory(out history);
                result.Result = new AgentResponse(history)
                {
                    Usage = usageContent?.Details,
                };

                //result.Result = await result.StreamResult.ToAgentResponseAsync();
                //Console.WriteLine(result.Result.Text);
            }

            /* Semantic Kernel 时代方法，已经启弃用）
            if (tempArguments != null && tempArguments.Count() != 0)
            {
                ////输入特定的本次请求临时上下文
                //if (useStream)
                //{
                //    result.StreamResult =  kernel.InvokeStreamingAsync(functionPipline.FirstOrDefault(), tempArguments);
                //}
                //else
                //{
                //    functionResult = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), tempArguments);
                //}
                //result.InputContext = new SenparcAiArguments(tempArguments);
            }
            else if (!prompt.IsNullOrEmpty())
            {
                //tempArguments 为空
                //输入纯文字
                if (functionPipline?.Length > 0)
                {
                    //使用 Pipleline
                    tempArguments = new() { ["input"] = prompt };

                    if (useStream)
                    {
                        //result.StreamResult = kernel.InvokeStreamingAsync(functionPipline.First(), tempArguments);
                    }
                    else
                    {
                        //TODO: 此方法在 NeuCharAI 接口中，不会给服务器传送 Body 内容
                        //functionResult = await kernel.InvokeAsync(functionPipline.First(), tempArguments);
                        agentResponse = await kernel.InvokeChatAsync(prompt);
                    }
                }
                else
                {
                    //不适用 Pipline

                    //注意：此处即使直接输入 prompt 作为第一个 String 参数，也会被封装到 Context，
                    //      并赋值给 Key 为 INPUT 的参数
                    //var kernelFunction = iWanToRun.CreateFunctionFromPrompt(prompt ?? "").function;

                    //if (useStream)
                    //{
                    //    result.StreamResult = kernel.InvokePromptStreamingAsync(prompt ?? "", storedArguments);
                    //}
                    //else
                    //{
                    //    functionResult = await kernel.InvokePromptAsync(prompt ?? "", storedArguments);
                    //}
                }

                result.InputContent = prompt;
            }
            else
            {
                //输入缓存中的上下文
                //botAnswer = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), storedArguments);

                //if (useStream)
                //{
                //    result.StreamResult = kernel.InvokeStreamingAsync(functionPipline.FirstOrDefault(), storedArguments);
                //}
                //else
                //{
                //    functionResult = await kernel.InvokeAsync(functionPipline.FirstOrDefault(), storedArguments);
                //}
                result.InputContext = new SenparcAiArguments(storedArguments);
            }

            result.InputContent = prompt;

            if (!useStream)
            {
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        result.OutputString = agentResponse.Text;//.GetValue<string>()?.TrimStart('\n') ?? "";
                    }
                    else
                    {
                        result.OutputString = agentResponse.RawRepresentation?.ToJson();//.GetValue<T>()?.ToJson()?.TrimStart('\n') ?? "";
                    }
                }
                catch (Exception)
                {
                    //TODO: 提供 Output 的泛型
                    result.OutputString = agentResponse.RawRepresentation?.ToJson()?.TrimStart('\n') ?? "";
                    _ = new SenparcAiException("无法转换为指定类型：" + typeof(T).Name);
                }
                result.Result = agentResponse;
            }
            else
            {
                var stringResult = new StringBuilder();

                if (result.StreamResult != null)
                {
                    await foreach (var item in result.StreamResult)
                    {
                        stringResult.Append(item);
                        inStreamItemProceessing?.Invoke(item);//执行流
                    }
                }

                result.OutputString = stringResult.ToString();
            }
            */
            //result.LastException = botAnswer.LastException;

            return result;
        }


        #endregion

        //#region Vision 模型运行

        ///// <summary>
        ///// 运行 Vision 模型
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">启用流，并指定遍历异步流每一步需要执行的委托。注意：只要此项不为 null，则会触发流式的请求。</param>
        ///// <returns></returns>
        //public static Task<SenparcKernelAiResult<string>> RunVisionAsync(this IWantToRun iWanToRun,
        //    SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    Action<StreamingKernelContent> inStreamItemProceessing = null)
        //{
        //    return RunVisionAsync<string>(iWanToRun, request, chatHistory, contentList, inStreamItemProceessing);
        //}

        ///// <summary>
        ///// 运行 Vision 模型
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">启用流，并指定遍历异步流每一步需要执行的委托。注意：只要此项不为 null，则会触发流式的请求。</param>
        ///// <typeparam name="T">指定返回结果类型</typeparam>
        ///// <returns></returns>

        //public static async Task<SenparcKernelAiResult<T>> RunVisionAsync<T>(this IWantToRun iWanToRun,
        //    SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    Action<StreamingKernelContent> inStreamItemProceessing = null)
        //{
        //    var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;

        //    var helper = iWanToRun.SemanticKernelHelper;
        //    var kernel = helper.GetKernel();
        //    //var function = iWanToRun.KernelFunction;

        //    var prompt = request.RequestContent;
        //    var functionPipline = request.FunctionPipeline;
        //    //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

        //    //注意：只要使用了 Plugin 和 Function，并且包含输入标识，就需要使用上下文

        //    iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
        //    var storedArguments = iWanToRun.StoredAiArguments.KernelArguments;
        //    var tempArguments = request.TempAiArguments?.KernelArguments;

        //    FunctionResult? functionResult = null;
        //    var result = new SenparcKernelAiResult<T>(iWanToRun, inputContent: null);

        //    var useStream = inStreamItemProceessing != null;

        //    var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        //    ChatMessageContentItemCollection contentItems = new ChatMessageContentItemCollection();
        //    foreach (var contentItem in contentList)
        //    {
        //        //if (contentItem.Type == Helpers.ContentType.Text)
        //        //{
        //        //    contentItems.Add(new TextContent(contentItem.TextContent));
        //        //}
        //        //else if (contentItem.Type == Helpers.ContentType.Image)
        //        //{
        //        //    contentItems.Add(new ImageContent_ImageBase64(contentItem.ImageData, "image/jpg"));
        //        //}
        //        if (contentItem is ContentItem_Text ciText)
        //        {
        //            contentItems.Add(new TextContent(ciText.TextContent));
        //        }
        //        else if (contentItem is ContentItem_ImageBse64 ciBae64)
        //        {
        //            contentItems.Add(new ImageContent(ciBae64.ImageData, "image/jpg"));
        //        }
        //        else if (contentItem is ContentItem_ImageUrl ciImageUrl)
        //        {
        //            contentItems.Add(new ImageContent("data:image/jpeg;base64," + ciImageUrl.image_url.Url));
        //        }
        //    }

        //    chatHistory.AddUserMessage(contentItems);

        //    var parameter = new PromptConfigParameter()
        //    {
        //        MaxTokens = 3500,
        //        Temperature = 0.7,
        //        TopP = 0.5,
        //    };
        //    PromptExecutionSettings? executionSettings = helper.GetExecutionSetting(parameter, helper.AiSetting);

        //    if (useStream)
        //    {
        //        result.StreamResult = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings: executionSettings, kernel: iWanToRun.Kernel);

        //        var stringResult = new StringBuilder();

        //        if (result.StreamResult != null)
        //        {
        //            await foreach (var item in result.StreamResult)
        //            {
        //                stringResult.Append(item);
        //                inStreamItemProceessing?.Invoke(item);//执行流
        //            }
        //        }

        //        result.OutputString = stringResult.ToString();
        //    }
        //    else
        //    {
        //        var contentResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings: executionSettings, kernel: iWanToRun.Kernel);
        //        //result.Result = contentResult;
        //        result.OutputString = contentResult.ToString();
        //    }

        //    return result;
        //}

        //#region Chat


        ///// <summary>
        ///// 运行 Chat + Vision 模型
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">启用流，并指定遍历异步流每一步需要执行的委托。注意：只要此项不为 null，则会触发流式的请求。</param>
        ///// <returns></returns>
        //public static Task<SenparcKernelAiResult<string>> RunChatVisionAsync(this IWantToRun iWanToRun,
        //    SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    PromptConfigParameter? parameter = null,
        //    Action<AgentResponseUpdate> inStreamItemProceessing = null)
        //{
        //    return RunChatVisionAsync<string>(iWanToRun, request, chatHistory, contentList, parameter, inStreamItemProceessing);
        //}

        ///// <summary>
        ///// 运行 Chat + Vision 模型
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="chatHistory"></param>
        ///// <param name="contentList"></param>
        ///// <param name="parameter"></param>
        ///// <param name="inStreamItemProceessing"></param>
        ///// <returns></returns>
        //public static async Task<SenparcKernelAiResult<T>> RunChatVisionAsync<T>(this IWantToRun iWanToRun,
        //   SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    PromptConfigParameter? parameter = null,
        //   Action<AgentResponseUpdate> inStreamItemProceessing = null)
        //{
        //    var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;

        //    var helper = iWanToRun.SemanticKernelHelper;
        //    var kernel = helper.GetKernel();
        //    //var function = iWanToRun.KernelFunction;

        //    //注意：只要使用了 Plugin 和 Function，并且包含输入标识，就需要使用上下文

        //    iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
        //    var storedArguments = iWanToRun.StoredAiArguments.KernelArguments;
        //    var tempArguments = request.TempAiArguments?.KernelArguments;

        //    FunctionResult? functionResult = null;
        //    var result = new SenparcKernelAiResult<T>(iWanToRun, inputContent: null);

        //    var useStream = inStreamItemProceessing != null;

        //    var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        //    ChatMessageContentItemCollection contentItems = new ChatMessageContentItemCollection();
        //    foreach (var contentItem in contentList)
        //    {
        //        //if (contentItem.Type == Helpers.ContentType.Text)
        //        //{
        //        //    contentItems.Add(new TextContent(contentItem.TextContent));
        //        //}
        //        //else if (contentItem.Type == Helpers.ContentType.Image)
        //        //{
        //        //    contentItems.Add(new ImageContent_ImageBase64(contentItem.ImageData, "image/jpg"));
        //        //}
        //        if (contentItem is ContentItem_Text ciText)
        //        {
        //            contentItems.Add(new TextContent(ciText.TextContent));
        //        }
        //        else if (contentItem is ContentItem_ImageBse64 ciBae64)
        //        {
        //            contentItems.Add(new ImageContent(ciBae64.ImageData, "image/jpg"));
        //        }
        //        else if (contentItem is ContentItem_ImageUrl ciImageUrl)
        //        {
        //            contentItems.Add(new ImageContent("data:image/jpeg;base64," + ciImageUrl.image_url.Url));
        //        }
        //    }

        //    chatHistory.AddUserMessage(contentItems);

        //    parameter ??= new PromptConfigParameter()
        //    {
        //        MaxTokens = 3500,
        //        Temperature = 0.7,
        //        TopP = 0.5,
        //    };
        //    PromptExecutionSettings? executionSettings = helper.GetExecutionSetting(parameter, helper.AiSetting);

        //    if (kernel.Plugins.Count > 0)
        //    {
        //        executionSettings.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
        //    }

        //    if (useStream)
        //    {
        //        result.StreamResult = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings: executionSettings, kernel: iWanToRun.Kernel);

        //        var stringResult = new StringBuilder();

        //        if (result.StreamResult != null)
        //        {
        //            await foreach (var item in result.StreamResult)
        //            {
        //                stringResult.Append(item);
        //                inStreamItemProceessing?.Invoke(item);//执行流
        //            }
        //        }

        //        result.OutputString = stringResult.ToString();
        //    }
        //    else
        //    {
        //        var contentResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings: executionSettings, kernel: iWanToRun.Kernel);
        //        //result.Result = contentResult;
        //        result.OutputString = contentResult.ToString();
        //    }

        //    return result;
        //}

        //#endregion

        //#endregion

    }
}
