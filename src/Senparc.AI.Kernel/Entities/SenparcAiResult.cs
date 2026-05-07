using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Handlers;

namespace Senparc.AI.Kernel;

public class SenparcAiResult : IAiResult
{
    public virtual string InputContent { get; set; } = string.Empty;
    public virtual IAiContext InputContext { get; set; } = new Entities.SenparcAiArguments();
    public virtual string OutputString { get; set; } = string.Empty;
    public virtual Exception? LastException { get; set; }
    public virtual IWantToRun IWantToRun { get; set; }
    public FunctionResultContent? LastFunctionResultContent { get; private set; }

    public SenparcAiResult(IWantToRun iwantToRun, string? inputContent)
    {
        IWantToRun = iwantToRun;
        InputContent = inputContent ?? string.Empty;
    }

    public SenparcAiResult(IWantToRun iwantToRun, IAiContext inputContext)
    {
        IWantToRun = iwantToRun;
        InputContext = inputContext;
    }

    public void SetLastFunctionResultContent(FunctionResultContent? functionResultContent = null)
    {
        LastFunctionResultContent = functionResultContent;
    }

    public (FunctionResultContent? FunctionResultContent, bool IsFunctionCall) GetLastFunctionResultContent()
    {
        var isFunctionCall = LastFunctionResultContent?.FunctionName != null;
        return (LastFunctionResultContent, isFunctionCall);
    }
}

public class SenparcAiResult<T> : SenparcAiResult, IAiResult
{
    public T? Result { get; set; }
    public IAsyncEnumerable<Microsoft.SemanticKernel.StreamingKernelContent>? StreamResult { get; set; }

    public SenparcAiResult(IWantToRun iWantToRun, string inputContent) : base(iWantToRun, inputContent)
    {
    }

    public SenparcAiResult(IWantToRun iWantToRun, IAiContext inputContext) : base(iWantToRun, inputContext)
    {
    }
}

public class SenparcKernelAiResult : SenparcKernelAiResult<string>, IAiResult
{
    public SenparcKernelAiResult(IWantToRun iWantToRun, string? inputContent) : base(iWantToRun, inputContent)
    {
    }
}

public class SenparcKernelAiResult<T> : SenparcAiResult<FunctionResult>, IAiResult
{
    public T? Output => Result is null ? default : Result.GetValue<T>();
    public new FunctionResult? Result { get; set; }
    public new IAsyncEnumerable<Microsoft.SemanticKernel.StreamingKernelContent>? StreamResult { get; set; }

    public SenparcKernelAiResult(IWantToRun iWantToRun, string? inputContent) : base(iWantToRun, inputContent ?? string.Empty)
    {
    }
}
