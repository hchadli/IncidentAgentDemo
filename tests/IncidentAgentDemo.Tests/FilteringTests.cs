using System.Net.Http.Json;
using IncidentAgentDemo.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IncidentAgentDemo.Tests;

public class FilteringTests : IClassFixture<WebApplicationFactory<Api.Program>>
{
    private readonly HttpClient _client;

    public FilteringTests(WebApplicationFactory<Api.Program> factory)
    {
        _client = TestWebApplicationFactory.CreateTestClient(factory, "FilterTestDb_" + Guid.NewGuid());
    }

    [Theory]
    [InlineData("Payments")]
    [InlineData("Identity")]
    [InlineData("Notifications")]
    public async Task FilterByService_ReturnsOnlyThatService(string serviceName)
    {
        var response = await _client.GetAsync($"/incidents/open?serviceName={serviceName}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenIncidentsResponse>();
        Assert.NotNull(result);
        Assert.Equal(serviceName, result.ServiceName);
        Assert.All(result.Incidents, i => Assert.Equal(serviceName, i.ServiceName));
    }

    [Fact]
    public async Task FilterByService_OnlyReturnsOpenIncidents()
    {
        var response = await _client.GetAsync("/incidents/open");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenIncidentsResponse>();
        Assert.NotNull(result);
        Assert.All(result.Incidents, i => Assert.Equal("Open", i.Status));
    }

    [Fact]
    public async Task OpenIncidents_AreOrderedByDateDescending()
    {
        var response = await _client.GetAsync("/incidents/open");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenIncidentsResponse>();
        Assert.NotNull(result);

        for (var i = 1; i < result.Incidents.Count; i++)
        {
            Assert.True(result.Incidents[i - 1].CreatedAtUtc >= result.Incidents[i].CreatedAtUtc,
                "Incidents should be ordered by date descending");
        }
    }
}
