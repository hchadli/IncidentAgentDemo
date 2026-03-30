using IncidentAgentDemo.McpServer;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentAgentDemo.AgentHost;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIncidentAgent(
        this IServiceCollection services,
        string apiBaseUrl,
        Action<PromptAssetsOptions>? configurePromptAssets = null)
    {
        services.AddHttpClient<IncidentApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Prompt asset services
        if (configurePromptAssets is not null)
            services.Configure(configurePromptAssets);
        else
            services.Configure<PromptAssetsOptions>(_ => { });

        services.AddSingleton<ISkillResolver, SkillResolver>();
        services.AddSingleton<IPromptAssetLoader, PromptAssetLoader>();
        services.AddSingleton<IPromptComposer, PromptComposer>();

        // Core agent services
        services.AddTransient<McpToolRegistry>();
        services.AddTransient<McpToolExecutor>();
        services.AddSingleton<OpenAiAgentClient>();
        services.AddTransient<IncidentAgentRunner>();

        return services;
    }
}
