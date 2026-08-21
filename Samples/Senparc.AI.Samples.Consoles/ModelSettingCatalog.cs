using Senparc.AI;
using Senparc.AI.Interfaces;
using Senparc.AI.Kernel;
using Senparc.CO2NET.Extensions;

/// <summary>
/// Model configuration options: Default, each platform *Keys section, and Items subsets.
/// </summary>
internal static class ModelSettingCatalog
{
    internal readonly record struct ModelChoice(string Key, string Label);

    private static readonly AiPlatform[] PlatformOrder =
    [
        AiPlatform.NeuCharAI,
        AiPlatform.AzureOpenAI,
        AiPlatform.OpenAI,
        AiPlatform.Ollama,
        AiPlatform.DeepSeek,
        AiPlatform.HuggingFace,
        AiPlatform.FastAPI,
    ];

    internal static IReadOnlyList<ModelChoice> GetChoices()
    {
        if (Senparc.AI.Config.SenparcAiSetting is not SenparcAiSetting root)
        {
            return [new ModelChoice("Default", "Default")];
        }

        var choices = new List<ModelChoice>
        {
            new("Default", $"Default(current default:{root.AiPlatform})")
        };

        foreach (var platform in PlatformOrder)
        {
            if (!IsPlatformConfigured(root, platform))
            {
                continue;
            }

            var modelHint = GetPlatformModelHint(root, platform);
            var label = modelHint.IsNullOrEmpty()
                ? $"{platform}({GetKeysSectionName(platform)})"
                : $"{platform}({GetKeysSectionName(platform)} / {modelHint})";
            choices.Add(new(platform.ToString(), label));
        }

        if (root.Items is { Count: > 0 })
        {
            foreach (var item in root.Items)
            {
                var itemSetting = item.Value;
                var chatModel = itemSetting.ModelName?.Chat;
                var label = chatModel.IsNullOrEmpty()
                    ? $"Items/{item.Key}({itemSetting.AiPlatform})"
                    : $"Items/{item.Key}({itemSetting.AiPlatform} / {chatModel})";
                choices.Add(new(item.Key, label));
            }
        }

        return choices;
    }

    internal static ISenparcAiSetting Resolve(string key)
    {
        var root = Senparc.AI.Config.SenparcAiSetting;
        if (key == "Default")
        {
            return root;
        }

        if (root is SenparcAiSetting setting && Enum.TryParse<AiPlatform>(key, out var platform))
        {
            return setting with { AiPlatform = platform };
        }

        return ((SenparcAiSetting)root)[key];
    }

    private static bool IsPlatformConfigured(SenparcAiSetting setting, AiPlatform platform) => platform switch
    {
        AiPlatform.NeuCharAI => setting.NeuCharAIKeys != null && !setting.NeuCharAIKeys.ApiKey.IsNullOrEmpty(),
        AiPlatform.AzureOpenAI => setting.AzureOpenAIKeys != null && !setting.AzureOpenAIKeys.ApiKey.IsNullOrEmpty(),
        AiPlatform.OpenAI => setting.IsOpenAiKeysSetted,
        AiPlatform.HuggingFace => setting.HuggingFaceKeys != null && !setting.HuggingFaceEndpoint.IsNullOrEmpty(),
        AiPlatform.FastAPI => setting.FastAPIKeys != null && !setting.FastAPIEndpoint.IsNullOrEmpty(),
        AiPlatform.Ollama => setting.OllamaKeys != null && !setting.OllamaEndpoint.IsNullOrEmpty(),
        AiPlatform.DeepSeek => setting.DeepSeekKeys != null && !setting.DeepSeekKeys.ApiKey.IsNullOrEmpty(),
        _ => false
    };

    private static string? GetPlatformModelHint(SenparcAiSetting setting, AiPlatform platform)
    {
        try
        {
            var clone = setting with { AiPlatform = platform };
            return clone.ModelName?.Chat;
        }
        catch
        {
            return null;
        }
    }

    private static string GetKeysSectionName(AiPlatform platform) => platform switch
    {
        AiPlatform.NeuCharAI => "NeuCharAIKeys",
        AiPlatform.AzureOpenAI => "AzureOpenAIKeys",
        AiPlatform.OpenAI => "OpenAIKeys",
        AiPlatform.HuggingFace => "HuggingFaceKeys",
        AiPlatform.FastAPI => "FastAPIKeys",
        AiPlatform.Ollama => "OllamaKeys",
        AiPlatform.DeepSeek => "DeepSeekKeys",
        _ => platform.ToString()
    };
}
