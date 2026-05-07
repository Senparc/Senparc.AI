using Microsoft.SemanticKernel;
using Senparc.AI.Entities;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel.Entities;
using Senparc.AI.Kernel.Handlers;

namespace Senparc.AI.Kernel;

public record SenparcAiRequest : IAiRequest<SenparcAiArguments>
{
    public IWantToRun IWantToRun { get; set; }
    public string UserId { get; set; }
    public string RequestContent { get; set; } = string.Empty;
    public PromptConfigParameter ParameterConfig { get; set; } = new();
    public SenparcAiArguments TempAiArguments { get; set; } = new();
    public SenparcAiArguments StoreAiArguments => IWantToRun.StoredAiArguments;
    public KernelFunction[] FunctionPipeline { get; set; }

    public SenparcAiRequest(IWantToRun iWantToRun, string userId, string requestContent, PromptConfigParameter parameterConfig, params KernelFunction[] pipeline)
    {
        IWantToRun = iWantToRun;
        UserId = userId;
        RequestContent = requestContent;
        ParameterConfig = parameterConfig;
        FunctionPipeline = pipeline;
    }

    public SenparcAiRequest(IWantToRun iWantToRun, string userId, KernelArguments contextVariables, PromptConfigParameter parameterConfig, params KernelFunction[] pipeline)
    {
        IWantToRun = iWantToRun;
        UserId = userId;
        ParameterConfig = parameterConfig;
        TempAiArguments = new SenparcAiArguments(contextVariables);
        FunctionPipeline = pipeline;
    }
}
