using Senparc.AI.Entities.Keys;
using Senparc.AI.Interfaces;

namespace Senparc.AI.Kernel.Entities;

public class SenparcAiSettingBuilder
{
    private readonly SenparcAiSetting _setting = new();

    public SenparcAiSettingBuilder ForPlatform(AiPlatform aiPlatform)
    {
        _setting.AiPlatform = aiPlatform;
        return this;
    }

    public SenparcAiSettingBuilder WithOpenAI(OpenAIKeys keys)
    {
        _setting.OpenAIKeys = keys;
        _setting.AiPlatform = AiPlatform.OpenAI;
        return this;
    }

    public SenparcAiSettingBuilder WithAzure(AzureOpenAIKeys keys)
    {
        _setting.AzureOpenAIKeys = keys;
        _setting.AiPlatform = AiPlatform.AzureOpenAI;
        return this;
    }

    public SenparcAiSettingBuilder AddModel(string key, ISenparcAiSetting value)
    {
        if (value is SenparcAiSetting setting)
        {
            _setting.Items[key] = setting;
        }
        return this;
    }

    public SenparcAiSetting Build() => _setting;
}
