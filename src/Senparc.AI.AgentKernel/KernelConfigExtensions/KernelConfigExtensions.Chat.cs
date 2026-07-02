using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Senparc.AI.AgentKernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        #region configuration

        public static ChatClientAgentOptions CreateChatClientAgentOptions(this IWantToConfig iWantToConfig,   string agentName, string systemMessage, ChatOptions chatOptions = null)
        {
            var options = new ChatClientAgentOptions()
            {
                Name = agentName,
                ChatOptions = chatOptions
            };

            return options;
        }

        #endregion

        #region run

        /// <summary>
        /// run
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunChatAsync(this IWantToRun iWanToRun, string prompt, AgentSession agentSession = null, Action<AgentResponseUpdate> inStreamItemProceessing = null)
        {
            return RunChatAsync<string>(iWanToRun, prompt, agentSession, inStreamItemProceessing);
        }

        /// <summary>
        /// run
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
        /// run
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunChatAsync(this IWantToRun iWanToRun, SenparcAiRequest request, Action<AgentResponseUpdate> inStreamItemProceessing = null)
        {
            return RunChatAsync<string>(iWanToRun, request, inStreamItemProceessing);
        }

        /// <summary>
        /// Run, compatible with streaming (unified RunChat entry point)
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        /// <typeparam name="T">Specify the return result type</typeparam>
        /// <returns></returns>

        public static async Task<SenparcKernelAiResult<T>> RunChatAsync<T>(this IWantToRun iWanToRun, SenparcAiRequest request, Action<AgentResponseUpdate> inStreamItemProceessing = null)
            where T : class
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.AgentKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            var prompt = request.RequestContent;

            //Replace parameter
            prompt = request.ReplacePrompt();

            var session = request.AgentSession;
            var functionPipline = request.FunctionPipeline;
            //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

            //Note: Context is required whenever Plugin and Function are used and an input identifier is included

            iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
            var storedArguments = iWanToRun.StoredAiArguments.AgentKernelArguments;
            var tempArguments = request.TempAiArguments?.AgentKernelArguments;

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
                    /* OpenAI may throw an exception when using JSON format:
                    Invalid parameter: 'response_format' of type 'json_schema' is not supported with this model
                    */

                    result.OutputString = ex.Message;
                    //TODO: Provide generic Output support
                    //result.OutputString = agentResponse.RawRepresentation?.ToJson()?.TrimStart('\n') ?? "";
                    _ = new SenparcAiException("Cannot convert to the specified type: " + typeof(T).Name);
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
                        inStreamItemProceessing?.Invoke(item);//Execute stream callback

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
            result.InputContent = prompt;

            #region MyRegion

            /* Semantic Kernel legacy method, already deprecated)
            if (tempArguments != null && tempArguments.Count() != 0)
            {
                ////Input the temporary context for this request
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
                //tempArguments is empty
                //Input plain text
                if (functionPipline?.Length > 0)
                {
                    //Use Pipeline
                    tempArguments = new() { ["input"] = prompt };

                    if (useStream)
                    {
                        //result.StreamResult = kernel.InvokeStreamingAsync(functionPipline.First(), tempArguments);
                    }
                    else
                    {
                        //TODO: This method does not send body content to the server in the NeuCharAI interface
                        //functionResult = await kernel.InvokeAsync(functionPipline.First(), tempArguments);
                        agentResponse = await kernel.InvokeChatAsync(prompt);
                    }
                }
                else
                {
                    //Do not use Pipeline

                    //note:Even if prompt is passed directly as the first string parameter here, it is wrapped into Context,
                    //      and assigned to the parameter whose key is INPUT
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
                //Input context from cache
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
                    //TODO: Provide generic Output support
                    result.OutputString = agentResponse.RawRepresentation?.ToJson()?.TrimStart('\n') ?? "";
                    _ = new SenparcAiException("Cannot convert to the specified type: " + typeof(T).Name);
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
                        inStreamItemProceessing?.Invoke(item);//Execute stream callback
                    }
                }

                result.OutputString = stringResult.ToString();
            }
            */
            //result.LastException = botAnswer.LastException;
            #endregion

            return result;
        }


        #endregion

        //#region Vision model run

        ///// <summary>
        ///// Run Vision model
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        ///// <returns></returns>
        //public static Task<SenparcKernelAiResult<string>> RunVisionAsync(this IWantToRun iWanToRun,
        //    SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    Action<StreamingKernelContent> inStreamItemProceessing = null)
        //{
        //    return RunVisionAsync<string>(iWanToRun, request, chatHistory, contentList, inStreamItemProceessing);
        //}

        ///// <summary>
        ///// Run Vision model
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        ///// <typeparam name="T">Specify the return result type</typeparam>
        ///// <returns></returns>

        //public static async Task<SenparcKernelAiResult<T>> RunVisionAsync<T>(this IWantToRun iWanToRun,
        //    SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    Action<StreamingKernelContent> inStreamItemProceessing = null)
        //{
        //    var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;

        //    var helper = iWanToRun.AgentKernelHelper;
        //    var kernel = helper.GetKernel();
        //    //var function = iWanToRun.KernelFunction;

        //    var prompt = request.RequestContent;
        //    var functionPipline = request.FunctionPipeline;
        //    //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

        //    //Note: Context is required whenever Plugin and Function are used and an input identifier is included

        //    iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
        //    var storedArguments = iWanToRun.StoredAiArguments.AgentKernelArguments;
        //    var tempArguments = request.TempAiArguments?.AgentKernelArguments;

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
        //                inStreamItemProceessing?.Invoke(item);//Execute stream callback
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
        ///// Run Chat + Vision model
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">Enable streaming and specify the delegate to execute for each async stream item. Note: any non-null value triggers a streaming request.</param>
        ///// <returns></returns>
        //public static Task<SenparcKernelAiResult<string>> RunChatVisionAsync(this IWantToRun iWanToRun,
        //    SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    PromptConfigParameter? parameter = null,
        //    Action<AgentResponseUpdate> inStreamItemProceessing = null)
        //{
        //    return RunChatVisionAsync<string>(iWanToRun, request, chatHistory, contentList, parameter, inStreamItemProceessing);
        //}

        ///// <summary>
        ///// Run Chat + Vision model
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

        //    var helper = iWanToRun.AgentKernelHelper;
        //    var kernel = helper.GetKernel();
        //    //var function = iWanToRun.KernelFunction;

        //    //Note: Context is required whenever Plugin and Function are used and an input identifier is included

        //    iWanToRun.StoredAiArguments ??= new SenparcAiArguments();
        //    var storedArguments = iWanToRun.StoredAiArguments.AgentKernelArguments;
        //    var tempArguments = request.TempAiArguments?.AgentKernelArguments;

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
        //                inStreamItemProceessing?.Invoke(item);//Execute stream callback
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
