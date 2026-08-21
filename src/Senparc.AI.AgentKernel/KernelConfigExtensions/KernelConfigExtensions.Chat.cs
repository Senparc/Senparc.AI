using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Entities;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Helpers;
using Senparc.AI.Entities;
using Senparc.AI.Exceptions;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Senparc.AI.AgentKernel.Handlers
{
    public static partial class KernelConfigExtensions
    {
        #region Configuration

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

        #region Run

        /// <summary>
        /// Executes the configured chat agent and preserves the provider exception for the caller.
        /// This keeps the IWantTo prompt replacement and ChatOptions sanitisation contract, while
        /// allowing protocol hosts such as A2A to return a real failure instead of an error string.
        /// </summary>
        public static Task<AgentResponse?> RunChatResponseAsync(
            this IWantToRun iWantToRun,
            string prompt,
            AgentSession? agentSession = null,
            ChatClientAgentRunOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var request = iWantToRun.CreateRequest(prompt, agentSession);
            return RunChatResponseAsync(iWantToRun, request, options, cancellationToken);
        }

        /// <summary>
        /// Executes a prepared request and preserves the provider exception for the caller.
        /// </summary>
        public static async Task<AgentResponse?> RunChatResponseAsync(
            this IWantToRun iWantToRun,
            SenparcAiRequest request,
            ChatClientAgentRunOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var execution = PrepareChatExecution(iWantToRun, request);
            return await execution.Kernel
                .InvokeChatAsync(execution.Prompt, execution.Session, options, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Streams the configured chat agent through the IWantTo execution contract.
        /// Provider exceptions are deliberately propagated so A2A, workflow and host code can keep
        /// the transport status distinct from a normal Agent response.
        /// </summary>
        public static async IAsyncEnumerable<AgentResponseUpdate> RunChatStreamingAsync(
            this IWantToRun iWantToRun,
            string prompt,
            AgentSession? agentSession = null,
            ChatClientAgentRunOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var request = iWantToRun.CreateRequest(prompt, agentSession);
            await foreach (var update in RunChatStreamingAsync(iWantToRun, request, options, cancellationToken)
                               .WithCancellation(cancellationToken)
                               .ConfigureAwait(false))
            {
                yield return update;
            }
        }

        /// <summary>
        /// Streams a prepared request through the IWantTo execution contract.
        /// </summary>
        public static async IAsyncEnumerable<AgentResponseUpdate> RunChatStreamingAsync(
            this IWantToRun iWantToRun,
            SenparcAiRequest request,
            ChatClientAgentRunOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var execution = PrepareChatExecution(iWantToRun, request);
            await foreach (var update in execution.Kernel
                               .InvokeChatStreamingAsync(execution.Prompt, execution.Session, options, cancellationToken)
                               .WithCancellation(cancellationToken)
                               .ConfigureAwait(false))
            {
                yield return update;
            }
        }

        private static (Senparc.AI.AgentKernel.Kernels.AiKernel Kernel, string Prompt, AgentSession? Session)
            PrepareChatExecution(IWantToRun iWantToRun, SenparcAiRequest request)
        {
            ArgumentNullException.ThrowIfNull(iWantToRun);
            ArgumentNullException.ThrowIfNull(request);

            var kernel = iWantToRun.Kernel
                ?? throw new SenparcAiException("IWantToRun has not built a chat kernel.");
            var prompt = request.ReplacePrompt() ?? string.Empty;
            var chatModelName = kernel.ModelName?.Chat
                ?? iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SenparcAiSetting?.ModelName?.Chat;
            ChatOptionsSanitizer.SanitizeForModel(kernel.ChatClientAgentOptions?.ChatOptions, chatModelName);

            return (kernel, prompt, request.AgentSession);
        }

        /// <summary>
        /// Runs the request.
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunChatAsync(this IWantToRun iWanToRun, string prompt, AgentSession agentSession = null, Action<AgentResponseUpdate> inStreamItemProceessing = null)
        {
            return RunChatAsync<string>(iWanToRun, prompt, agentSession, inStreamItemProceessing);
        }

        /// <summary>
        /// Runs the request.
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
        /// Runs the request.
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enables streaming and specifies the delegate to execute for each step of the asynchronous stream. A non-null value triggers a streaming request.</param>
        /// <returns></returns>
        public static Task<SenparcKernelAiResult<string>> RunChatAsync(this IWantToRun iWanToRun, SenparcAiRequest request, Action<AgentResponseUpdate> inStreamItemProceessing = null)
        {
            return RunChatAsync<string>(iWanToRun, request, inStreamItemProceessing);
        }

        /// <summary>
        /// Runs the request with streaming compatibility through the unified RunChat entry point.
        /// </summary>
        /// <param name="iWanToRun"></param>
        /// <param name="request"></param>
        /// <param name="inStreamItemProceessing">Enables streaming and specifies the delegate to execute for each step of the asynchronous stream. A non-null value triggers a streaming request.</param>
        /// <typeparam name="T">The specified result type.</typeparam>
        /// <returns></returns>

        public static async Task<SenparcKernelAiResult<T>> RunChatAsync<T>(this IWantToRun iWanToRun, SenparcAiRequest request, Action<AgentResponseUpdate> inStreamItemProceessing = null)
            where T : class
        {
            var iWantTo = iWanToRun.IWantToBuild.IWantToConfig.IWantTo;
            var helper = iWanToRun.AgentKernelHelper;
            var kernel = helper.GetKernel();
            //var function = iWanToRun.KernelFunction;

            var prompt = request.RequestContent;

            // Replace parameters.
            prompt = request.ReplacePrompt();

            var session = request.AgentSession;
            var functionPipline = request.FunctionPipeline;
            //var serviceId = helper.GetServiceId(iWantTo.UserId, iWantTo.ModelName);

            // For GPT-5+ and similar models, ensure again that sampling parameters such as Temperature are absent before submission.
            var chatModelName = kernel.ModelName?.Chat ?? iWantTo.SenparcAiSetting?.ModelName?.Chat;
            ChatOptionsSanitizer.SanitizeForModel(kernel.ChatClientAgentOptions?.ChatOptions, chatModelName);

            // When using a Plugin and Function with an input identifier, context is required.

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
                    result.LastException = ex;
                    // TODO: Provide a generic Output type.
                    //result.OutputString = agentResponse.RawRepresentation?.ToJson()?.TrimStart('\n') ?? "";
                    _ = new SenparcAiException("Unable to convert to the specified type: " + typeof(T).Name);
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
                        inStreamItemProceessing?.Invoke(item);// Execute the stream.

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

            /* Semantic Kernel-era method; deprecated.
            if (tempArguments != null && tempArguments.Count() != 0)
            {
                //// Enter the temporary context specific to this request.
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
                // tempArguments is empty.
                // Enter plain text.
                if (functionPipline?.Length > 0)
                {
                    // Use the pipeline.
                    tempArguments = new() { ["input"] = prompt };

                    if (useStream)
                    {
                        //result.StreamResult = kernel.InvokeStreamingAsync(functionPipline.First(), tempArguments);
                    }
                    else
                    {
                        // TODO: With the NeuCharAI API, this method does not send Body content to the server.
                        //functionResult = await kernel.InvokeAsync(functionPipline.First(), tempArguments);
                        agentResponse = await kernel.InvokeChatAsync(prompt);
                    }
                }
                else
                {
                    // Do not use the pipeline.

                    // Even when prompt is passed directly as the first String parameter, it is wrapped in Context
                    // and assigned to the parameter whose Key is INPUT.
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
                // Enter the context from the cache.
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
                    // TODO: Provide a generic Output type.
                    result.OutputString = agentResponse.RawRepresentation?.ToJson()?.TrimStart('\n') ?? "";
                    _ = new SenparcAiException("Unable to convert to the specified type: " + typeof(T).Name);
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
                        inStreamItemProceessing?.Invoke(item);// Execute the stream.
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

        //#region Vision Model Execution

        ///// <summary>
        ///// Runs a Vision model.
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">Enables streaming and specifies the delegate to execute for each step of the asynchronous stream. A non-null value triggers a streaming request.</param>
        ///// <returns></returns>
        //public static Task<SenparcKernelAiResult<string>> RunVisionAsync(this IWantToRun iWanToRun,
        //    SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    Action<StreamingKernelContent> inStreamItemProceessing = null)
        //{
        //    return RunVisionAsync<string>(iWanToRun, request, chatHistory, contentList, inStreamItemProceessing);
        //}

        ///// <summary>
        ///// Runs a Vision model.
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">Enables streaming and specifies the delegate to execute for each step of the asynchronous stream. A non-null value triggers a streaming request.</param>
        ///// <typeparam name="T">The specified result type.</typeparam>
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

        //    // When using a Plugin and Function with an input identifier, context is required.

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
        //                inStreamItemProceessing?.Invoke(item);// Execute the stream.
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
        ///// Runs a Chat + Vision model.
        ///// </summary>
        ///// <param name="iWanToRun"></param>
        ///// <param name="request"></param>
        ///// <param name="inStreamItemProceessing">Enables streaming and specifies the delegate to execute for each step of the asynchronous stream. A non-null value triggers a streaming request.</param>
        ///// <returns></returns>
        //public static Task<SenparcKernelAiResult<string>> RunChatVisionAsync(this IWantToRun iWanToRun,
        //    SenparcAiRequest request, ChatHistory chatHistory, List<IContentItem> contentList,
        //    PromptConfigParameter? parameter = null,
        //    Action<AgentResponseUpdate> inStreamItemProceessing = null)
        //{
        //    return RunChatVisionAsync<string>(iWanToRun, request, chatHistory, contentList, parameter, inStreamItemProceessing);
        //}

        ///// <summary>
        ///// Runs a Chat + Vision model.
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

        //    // When using a Plugin and Function with an input identifier, context is required.

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
        //                inStreamItemProceessing?.Invoke(item);// Execute the stream.
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
