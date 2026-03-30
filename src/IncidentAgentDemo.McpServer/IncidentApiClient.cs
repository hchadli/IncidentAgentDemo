using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IncidentAgentDemo.Contracts;

namespace IncidentAgentDemo.McpServer;

public sealed class IncidentApiClient(HttpClient httpClient)
{
    public async Task<OpenIncidentsResponse?> GetOpenIncidentsAsync(string serviceName, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(serviceName)
            ? "/incidents/open"
            : $"/incidents/open?serviceName={Uri.EscapeDataString(serviceName)}";

        return await httpClient.GetFromJsonAsync<OpenIncidentsResponse>(url, ct);
    }

    public async Task<IncidentDto?> GetIncidentByIdAsync(int id, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<IncidentDto>($"/incidents/{id}", ct);
    }

    public async Task<ServiceHealthDto?> GetServiceHealthAsync(string serviceName, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<ServiceHealthDto>(
            $"/services/{Uri.EscapeDataString(serviceName)}/health", ct);
    }

    public async Task<(IncidentDto? Incident, string? Error)> CreateIncidentAsync(
        CreateIncidentRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/incidents", request, ct);

        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<IncidentDto>(ct);
            return (dto, null);
        }

        var error = await TryReadError(response, ct);
        return (null, error);
    }

    public async Task<(IncidentDto? Incident, string? Error)> CloseIncidentAsync(
        int id, CloseIncidentRequest? request = null, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/incidents/{id}/close", request ?? new(), ct);

        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<IncidentDto>(ct);
            return (dto, null);
        }

        var error = await TryReadError(response, ct);
        return (null, error);
    }

    private static async Task<string> TryReadError(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            if (body.TryGetProperty("error", out var errorProp))
                return errorProp.GetString() ?? $"HTTP {(int)response.StatusCode}";
        }
        catch
        {
            // Ignore deserialization failures
        }

        return $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
    }
}
