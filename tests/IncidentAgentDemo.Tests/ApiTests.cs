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

    // === Create Incident Tests ===

    [Fact]
    public async Task CreateIncident_ValidRequest_Returns201WithIncident()
    {
        var request = new CreateIncidentRequest(
            "Test outage", "Payments", "High", "Payment gateway is down.");

        var response = await _client.PostAsJsonAsync("/incidents", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test outage", result.Title);
        Assert.Equal("Payments", result.ServiceName);
        Assert.Equal("High", result.Severity);
        Assert.Equal("Open", result.Status);
        Assert.Equal("Payment gateway is down.", result.Summary);
    }

    [Fact]
    public async Task CreateIncident_MissingTitle_Returns400()
    {
        var request = new CreateIncidentRequest("", "Payments", "High", "Some summary");

        var response = await _client.PostAsJsonAsync("/incidents", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateIncident_InvalidSeverity_Returns400()
    {
        var request = new CreateIncidentRequest("Title", "Payments", "Extreme", "Some summary");

        var response = await _client.PostAsJsonAsync("/incidents", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // === Close Incident Tests ===

    [Fact]
    public async Task CloseIncident_ExistingOpenIncident_ReturnsClosedIncident()
    {
        // First, create an incident to close
        var createRequest = new CreateIncidentRequest(
            "To close", "Identity", "Low", "Will be closed.");
        var createResponse = await _client.PostAsJsonAsync("/incidents", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(created);

        var closeRequest = new CloseIncidentRequest("Fixed by rollback");
        var response = await _client.PostAsJsonAsync($"/incidents/{created.Id}/close", closeRequest);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(result);
        Assert.Equal("Closed", result.Status);
        Assert.Equal("Fixed by rollback", result.ResolutionNote);
        Assert.NotNull(result.ClosedAtUtc);
    }

    [Fact]
    public async Task CloseIncident_NonExistingId_Returns404()
    {
        var closeRequest = new CloseIncidentRequest();
        var response = await _client.PostAsJsonAsync("/incidents/9999/close", closeRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CloseIncident_AlreadyClosed_Returns400()
    {
        // Create and close an incident first
        var createRequest = new CreateIncidentRequest(
            "Already closed", "Notifications", "Medium", "Will be closed twice.");
        var createResponse = await _client.PostAsJsonAsync("/incidents", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<IncidentDto>();
        Assert.NotNull(created);

        await _client.PostAsJsonAsync($"/incidents/{created.Id}/close", new CloseIncidentRequest());

        // Try to close again
        var response = await _client.PostAsJsonAsync($"/incidents/{created.Id}/close", new CloseIncidentRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
