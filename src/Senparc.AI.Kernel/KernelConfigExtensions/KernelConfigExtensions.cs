using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Senparc.AI.Entities;
using Senparc.AI.Entities.Keys;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;

namespace Senparc.AI.Kernel.Handlers;

public class ImportPluginResult
{
    public string PluginName { get; set; } = string.Empty;
}

public class CompatKernelPlugin
{
    private readonly Dictionary<string, KernelFunction> _functions = new();

    public KernelFunction this[string functionName]
    {
        get
        {
            if (!_functions.TryGetValue(functionName, out var function))
            {
                function = new KernelFunction { Name = functionName };
                _functions[functionName] = function;
            }
            return function;
        }
    }
}

public class CompatVectorSearchResult<TRecord>
{
    public TRecord Record { get; set; } = default!;
    public float Score { get; set; }
}

public class CompatVectorCollection<TKey, TRecord> where TKey : notnull where TRecord : class
{
    private readonly List<TRecord> _items = [];

    public Task EnsureCollectionExistsAsync() => Task.CompletedTask;

    public Task UpsertAsync(TRecord record)
    {
        _items.Add(record);
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<CompatVectorSearchResult<TRecord>> SearchAsync(ReadOnlyMemory<float> searchVector, int top)
    {
        var count = 0;
        foreach (var item in _items)
        {
            yield return new CompatVectorSearchResult<TRecord>
            {
                Record = item,
                Score = 1f / (count + 1)
            };
            count++;
            if (count >= top)
            {
                break;
            }
        }
        await Task.CompletedTask;
    }
}

public static partial class KernelConfigExtension
{
    public static IWantToConfig IWantTo(this SemanticAiHandler handler, ISenparcAiSetting? senparcAiSetting = null)
    {
        return new IWantToConfig(new IWantTo(handler, senparcAiSetting));
    }

    public static IWantToConfig ConfigModel(this IWantToConfig iWantToConfig, ConfigModel configModel, string userId, ModelName? modelName = null, ISenparcAiSetting? senparcAiSetting = null, string? deploymentName = null)
    {
        var targetSetting = senparcAiSetting ?? iWantToConfig.IWantTo.SenparcAiSetting;
        modelName ??= targetSetting.ModelName;

        iWantToConfig.UserId = userId;
        iWantToConfig.ModelName = configModel switch
        {
            Senparc.AI.ConfigModel.Chat => modelName.Chat,
            Senparc.AI.ConfigModel.TextEmbedding => modelName.Embedding,
            Senparc.AI.ConfigModel.TextToImage => modelName.TextToImage,
            Senparc.AI.ConfigModel.SpeechToText => modelName.SpeechToText,
            _ => modelName.TextCompletion
        } ?? string.Empty;

        iWantToConfig.IWantTo.UserId = userId;
        iWantToConfig.IWantTo.ModelName = iWantToConfig.ModelName;
        iWantToConfig.IWantTo.SenparcAiSetting = targetSetting;
        return iWantToConfig;
    }

    public static IWantToRun BuildKernel(this IWantToConfig iWantToConfig, Action<IKernelBuilder>? kernelBuilderAction = null)
    {
        kernelBuilderAction?.Invoke(iWantToConfig.IWantTo.KernelBuilder);
        return new IWantToRun(new IWantToBuild(iWantToConfig));
    }

    public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, string? requestContent, bool useAllRegisteredFunctions = false, params KernelFunction[] pipeline)
    {
        var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;
        var allPipelines = useAllRegisteredFunctions ? iWantToRun.Functions.Concat(pipeline).ToArray() : pipeline;
        return new SenparcAiRequest(iWantToRun, iWantTo.UserId, requestContent ?? string.Empty, iWantToRun.PromptConfigParameter ?? new PromptConfigParameter(), allPipelines);
    }

    public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, bool useAllRegisteredFunctions = false, params KernelFunction[] pipeline)
    {
        return CreateRequest(iWantToRun, requestContent: null, useAllRegisteredFunctions, pipeline);
    }

    public static SenparcAiRequest CreateRequest(this IWantToRun iWantToRun, KernelArguments arguments, bool useAllRegisteredFunctions = false, params KernelFunction[] pipeline)
    {
        var iWantTo = iWantToRun.IWantToBuild.IWantToConfig.IWantTo;
        var allPipelines = useAllRegisteredFunctions ? iWantToRun.Functions.Concat(pipeline).ToArray() : pipeline;
        return new SenparcAiRequest(iWantToRun, iWantTo.UserId, arguments, iWantToRun.PromptConfigParameter ?? new PromptConfigParameter(), allPipelines);
    }

    public static SenparcAiRequest SetTempContext(this SenparcAiRequest request, string key, string value)
    {
        request.TempAiArguments.KernelArguments.Set(key, value);
        return request;
    }

    public static SenparcAiRequest SetStoredContext(this SenparcAiRequest request, string key, object value)
    {
        request.StoreAiArguments.KernelArguments.Set(key, value);
        return request;
    }

    public static bool GetTempArguments(this SenparcAiRequest request, string key, out object? value)
    {
        return request.TempAiArguments.KernelArguments.TryGetValue(key, out value);
    }

    public static bool GetStoredArguments(this SenparcAiRequest request, string key, out object? value)
    {
        return request.StoreAiArguments.KernelArguments.TryGetValue(key, out value);
    }

    public static Task<SenparcKernelAiResult<string>> RunAsync(this IWantToRun iWantToRun, SenparcAiRequest request, Action<Microsoft.SemanticKernel.StreamingKernelContent>? inStreamItemProceessing = null)
    {
        return RunAsync<string>(iWantToRun, request, inStreamItemProceessing);
    }

    public static async Task<SenparcKernelAiResult<T>> RunAsync<T>(this IWantToRun iWantToRun, SenparcAiRequest request, Action<Microsoft.SemanticKernel.StreamingKernelContent>? inStreamItemProceessing = null)
    {
        var result = await iWantToRun.IWantToBuild.IWantToConfig.IWantTo.SemanticAiHandler.ChatAsync(
            iWantToRun,
            request.RequestContent,
            inStreamItemProceessing,
            parameter: request.ParameterConfig);

        return new SenparcKernelAiResult<T>(iWantToRun, request.RequestContent)
        {
            OutputString = result.OutputString,
            Result = new FunctionResult { Value = result.OutputString }
        };
    }

    public static Task<SenparcKernelAiResult<string>> RunStreamAsync(this IWantToRun iWantToRun, SenparcAiRequest request, Action<Microsoft.SemanticKernel.StreamingKernelContent>? inStreamItemProceessing = null)
    {
        inStreamItemProceessing ??= _ => { };
        return RunAsync<string>(iWantToRun, request, inStreamItemProceessing);
    }

    public static Task<SenparcKernelAiResult> RunAsync(this IWantToRun iWantToRun, KernelFunction kernelFunction)
    {
        return Task.FromResult(new SenparcKernelAiResult(iWantToRun, kernelFunction.Name)
        {
            OutputString = kernelFunction.Name,
            Result = new FunctionResult { Value = kernelFunction.Name }
        });
    }

    public static Task<SenparcAiResult<T>> RunAsync<T>(this IWantToRun iWantToRun, KernelFunction kernelFunction)
    {
        return Task.FromResult(new SenparcAiResult<T>(iWantToRun, kernelFunction.Name)
        {
            OutputString = kernelFunction.Name
        });
    }

    public static (IWantToRun iWantToRun, KernelFunction function) CreateFunctionFromPrompt(
        this IWantToRun iWantToRun,
        string prompt,
        string? functionName = null,
        string? pluginName = null,
        int? maxTokens = null,
        double? temperature = null,
        double? topP = null,
        PromptConfigParameter? promptConfigPara = null)
    {
        var function = new KernelFunction { Name = functionName ?? "CompatFunction" };
        iWantToRun.Functions.Add(function);
        iWantToRun.PromptConfigParameter = promptConfigPara ?? new PromptConfigParameter
        {
            MaxTokens = maxTokens,
            Temperature = temperature,
            TopP = topP
        };
        return (iWantToRun, function);
    }

    public static ImportPluginResult ImportPluginFromPromptDirectory(this IWantToRun iWantToRun, string parentDirectory, string pluginName)
    {
        return new ImportPluginResult { PluginName = pluginName };
    }

    public static (IWantToRun iWantToRun, CompatKernelPlugin kernelPlugin) ImportPluginFromObject(this IWantToRun iWantToRun, object target, string? pluginName = null)
    {
        return (iWantToRun, new CompatKernelPlugin());
    }

    public static (IWantToRun iWantToRun, KernelArguments arguments) CreateNewArguments(this IWantToRun iWantToRun)
    {
        return (iWantToRun, new KernelArguments());
    }

    public static IWantToConfig ConfigVectorStore(this IWantToConfig iWantToConfig, VectorDB vectorDb)
    {
        return iWantToConfig;
    }

    public static CompatVectorCollection<TKey, TRecord> GetVectorCollection<TKey, TRecord>(this IWantToRun iWantToRun, VectorDB vectorDb, string name)
        where TKey : notnull where TRecord : class
    {
        return new CompatVectorCollection<TKey, TRecord>();
    }
}
