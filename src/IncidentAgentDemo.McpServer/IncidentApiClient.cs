using System.Net.Http.Json;
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
}
