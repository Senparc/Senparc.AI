using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;
using Senparc.AI.Kernel.Helpers;

namespace Senparc.AI.Kernel;

public class SemanticAiHandler : IAiHandler<SenparcAiRequest, SenparcAiResult, SenparcAiArguments>
{
    private readonly MafRuntimeClient _runtimeClient;
    public ISenparcAiSetting SenparcAiSetting { get; }
    public SemanticKernelHelper SemanticKernelHelper { get; }

    public SemanticAiHandler(ISenparcAiSetting senparcAiSetting, HttpClient? httpClient = null)
    {
        SenparcAiSetting = senparcAiSetting;
        SemanticKernelHelper = new SemanticKernelHelper(senparcAiSetting);
        _runtimeClient = new MafRuntimeClient(httpClient);
    }

    public SenparcAiResult Run(SenparcAiRequest request, ISenparcAiSetting? senparcAiSetting = null)
    {
        var result = ChatAsync(request.IWantToRun, request.RequestContent, null, parameter: request.ParameterConfig).GetAwaiter().GetResult();
        return result;
    }

    public IWantToRun ChatConfig(
        PromptConfigParameter promptConfigParameter,
        string userId,
        int maxHistoryStore,
        ModelName? modelName = null,
        string? chatSystemMessage = null,
        string? promptTemplate = null,
        ISenparcAiSetting? senparcAiSetting = null,
        Action<IKernelBuilder>? kernelBuilderAction = null,
        string humanId = "User", string robotId = "Assistant", string hisgoryArgName = "history", string humanInputArgName = "human_input")
    {
        var iWantToConfig = this.IWantTo(senparcAiSetting).ConfigModel(ConfigModel.Chat, userId, modelName);
        kernelBuilderAction?.Invoke(iWantToConfig.IWantTo.KernelBuilder);
        var iWantToRun = iWantToConfig.BuildKernel();

        iWantToRun.PromptConfigParameter = promptConfigParameter;
        iWantToRun.IWantToBuild.IWantToConfig.IWantTo.TempStore["MaxHistoryCount"] = maxHistoryStore;

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(chatSystemMessage ?? "You are a helpful assistant.");
        iWantToRun.StoredAiArguments.KernelArguments.Set(hisgoryArgName, chatHistory);
        iWantToRun.StoredAiArguments.KernelArguments.Set("promptTemplate", promptTemplate ?? string.Empty);

        return iWantToRun;
    }

    public async Task<SenparcAiResult> ChatAsync(
        IWantToRun iWantToRun,
        string input,
        Action<Microsoft.SemanticKernel.StreamingKernelContent>? inStreamItemProceessing = null,
        string humanId = "User",
        string robotId = "Assistant",
        string historyArgName = "history",
        string humanInputArgName = "human_input",
        PromptConfigParameter? parameter = null)
    {
        var request = iWantToRun.CreateRequest(true);
        if (!request.GetStoredArguments(historyArgName, out var historyObj) || historyObj is not ChatHistory history)
        {
            history = new ChatHistory();
            request.SetStoredContext(historyArgName, history);
        }

        history.AddUserMessage(input);

        var modelName = iWantToRun.IWantToBuild.IWantToConfig.ModelName;
        var text = await _runtimeClient.ChatAsync(
            iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SenparcAiSetting,
            modelName,
            input,
            parameter ?? iWantToRun.PromptConfigParameter);

        if (inStreamItemProceessing is not null)
        {
            foreach (var chunk in ChunkText(text, 24))
            {
                inStreamItemProceessing(new Microsoft.SemanticKernel.StreamingKernelContent { Content = chunk });
            }
        }

        history.AddAssistantMessage(text);
        if (iWantToRun.IWantToBuild.IWantToConfig.IWantTo.TempStore.TryGetValue("MaxHistoryCount", out var maxObj) && maxObj is int maxHistoryCount)
        {
            RemoveHistory(history, maxHistoryCount);
        }

        return new SenparcKernelAiResult(iWantToRun, input)
        {
            OutputString = text,
            Result = new FunctionResult { Value = text }
        };
    }

    public string RemoveHistory(string history, int maxHistoryCount, string humanId = "Human", string robotId = "ChatBot")
    {
        var pattern = $@"{humanId}:.*?{robotId}:.*?(?=({humanId}:|$))";
        var matches = Regex.Matches(history, pattern, RegexOptions.Singleline);
        if (matches.Count <= maxHistoryCount)
        {
            return history;
        }

        var removeCount = matches.Count - maxHistoryCount;
        var count = 0;
        return Regex.Replace(history, pattern, m => ++count <= removeCount ? "" : m.Value, RegexOptions.Singleline);
    }

    public void RemoveHistory(ChatHistory chatHistory, int maxHistoryCount)
    {
        if (maxHistoryCount <= 0 || chatHistory.Count <= maxHistoryCount * 2 + 1)
        {
            return;
        }

        while (chatHistory.Count > maxHistoryCount * 2 + 1)
        {
            if (chatHistory.Count > 1)
            {
                chatHistory.RemoveAt(1);
            }
            if (chatHistory.Count > 1)
            {
                chatHistory.RemoveAt(1);
            }
        }
    }

    private static IEnumerable<string> ChunkText(string text, int chunkSize)
    {
        for (var i = 0; i < text.Length; i += chunkSize)
        {
            yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
        }
    }
}
