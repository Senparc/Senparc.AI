using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.AI.Interfaces;

namespace Senparc.AI.Kernel;

public static class Register
{
    public static IServiceCollection AddSenparcAI(this IServiceCollection services, ISenparcAiSetting senparcAiSetting)
    {
        Config.SenparcAiSetting = senparcAiSetting;
        Senparc.AI.Config.SenparcAiSetting = senparcAiSetting;
        services.AddSingleton(senparcAiSetting);
        services.AddSingleton<SemanticAiHandler>();
        return services;
    }

    public static string GetAppSettingsFile(IConfiguration configuration, string defaultFile = "appsettings.json")
    {
        return configuration["SenparcAiSetting:ConfigFile"] ?? defaultFile;
    }
}
