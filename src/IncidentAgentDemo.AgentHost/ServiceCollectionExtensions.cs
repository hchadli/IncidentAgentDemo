using IncidentAgentDemo.McpServer;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentAgentDemo.AgentHost;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIncidentAgent(this IServiceCollection services, string apiBaseUrl)
    {
        services.AddHttpClient<IncidentApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddTransient<McpToolRegistry>();
        services.AddTransient<McpToolExecutor>();
        services.AddSingleton<OpenAiAgentClient>();
        services.AddTransient<IncidentAgentRunner>();

        return services;
    }
}
