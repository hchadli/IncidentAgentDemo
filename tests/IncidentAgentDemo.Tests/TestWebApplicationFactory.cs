using IncidentAgentDemo.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentAgentDemo.Tests;

internal static class TestWebApplicationFactory
{
    public static HttpClient CreateTestClient(WebApplicationFactory<Api.Program> factory, string dbName)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all EF Core registrations for IncidentDbContext
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<IncidentDbContext>)
                             || d.ServiceType == typeof(DbContextOptions)
                             || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                    .ToList();

                foreach (var d in descriptors)
                    services.Remove(d);

                services.AddDbContext<IncidentDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        }).CreateClient();
    }
}
