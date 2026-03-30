using System.Net;
using System.Net.Http.Json;
using IncidentAgentDemo.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IncidentAgentDemo.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Api.Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Api.Program> factory)
    {
        _client = TestWebApplicationFactory.CreateTestClient(factory, "ApiTestDb_" + Guid.NewGuid());
    }

    [Fact]
    public async Task GetOpenIncidents_ReturnsSeededData()
    {
        var response = await _client.GetAsync("/incidents/open");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenIncidentsResponse>();
        Assert.NotNull(result);
        Assert.True(result.Count > 0, "Should have seeded open incidents");
    }

    [Fact]
    public async Task GetOpenIncidents_FilterByService_ReturnsOnlyMatchingService()
    {
        var response = await _client.GetAsync("/incidents/open?serviceName=Payments");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenIncidentsResponse>();
        Assert.NotNull(result);
        Assert.Equal("Payments", result.ServiceName);
        Assert.All(result.Incidents, i => Assert.Equal("Payments", i.ServiceName));
    }

    [Fact]
    public async Task GetIncidentById_ExistingId_ReturnsIncident()
    {
        var response = await _client.GetAsync("/incidents/1");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.False(string.IsNullOrEmpty(result.Title));
    }

    [Fact]
    public async Task GetIncidentById_NonExistingId_Returns404()
    {
        var response = await _client.GetAsync("/incidents/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetServiceHealth_ExistingService_ReturnsHealth()
    {
        var response = await _client.GetAsync("/services/Payments/health");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ServiceHealthDto>();
        Assert.NotNull(result);
        Assert.Equal("Payments", result.ServiceName);
        Assert.False(string.IsNullOrEmpty(result.Status));
    }

    [Fact]
    public async Task GetServiceHealth_NonExistingService_Returns404()
    {
        var response = await _client.GetAsync("/services/NonExistent/health");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
