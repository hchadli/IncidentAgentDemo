using System.Text.Json;
using IncidentAgentDemo.Contracts;

namespace IncidentAgentDemo.McpServer;

public sealed class McpToolRegistry(IncidentApiClient apiClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public IReadOnlyList<string> ToolNames =>
    [
        ToolDefinitions.GetOpenIncidents,
        ToolDefinitions.GetIncidentById,
        ToolDefinitions.GetServiceHealth,
        ToolDefinitions.CreateIncident,
        ToolDefinitions.CloseIncident
    ];

    public async Task<string> ExecuteToolAsync(string toolName, string argumentsJson, CancellationToken ct = default)
    {
        try
        {
            var args = JsonSerializer.Deserialize<JsonElement>(argumentsJson);

            return toolName switch
            {
                ToolDefinitions.GetOpenIncidents => await ExecuteGetOpenIncidentsAsync(args, ct),
                ToolDefinitions.GetIncidentById => await ExecuteGetIncidentByIdAsync(args, ct),
                ToolDefinitions.GetServiceHealth => await ExecuteGetServiceHealthAsync(args, ct),
                ToolDefinitions.CreateIncident => await ExecuteCreateIncidentAsync(args, ct),
                ToolDefinitions.CloseIncident => await ExecuteCloseIncidentAsync(args, ct),
                _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolName}" })
            };
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<string> ExecuteGetOpenIncidentsAsync(JsonElement args, CancellationToken ct)
    {
        var serviceName = args.TryGetProperty("serviceName", out var sn)
            ? sn.GetString() ?? ""
            : "";

        var result = await apiClient.GetOpenIncidentsAsync(serviceName, ct);
        return JsonSerializer.Serialize(result, JsonOptions);
    }

    private async Task<string> ExecuteGetIncidentByIdAsync(JsonElement args, CancellationToken ct)
    {
        if (!args.TryGetProperty("id", out var idProp))
            return JsonSerializer.Serialize(new { error = "Missing required parameter: id" });

        var id = idProp.GetInt32();
        var result = await apiClient.GetIncidentByIdAsync(id, ct);

        return result is not null
            ? JsonSerializer.Serialize(result, JsonOptions)
            : JsonSerializer.Serialize(new { error = $"Incident {id} not found" });
    }

    private async Task<string> ExecuteGetServiceHealthAsync(JsonElement args, CancellationToken ct)
    {
        if (!args.TryGetProperty("serviceName", out var snProp))
            return JsonSerializer.Serialize(new { error = "Missing required parameter: serviceName" });

        var serviceName = snProp.GetString() ?? "";
        var result = await apiClient.GetServiceHealthAsync(serviceName, ct);

        return result is not null
            ? JsonSerializer.Serialize(result, JsonOptions)
            : JsonSerializer.Serialize(new { error = $"Service '{serviceName}' not found" });
    }

    private async Task<string> ExecuteCreateIncidentAsync(JsonElement args, CancellationToken ct)
    {
        var title = args.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
        var serviceName = args.TryGetProperty("serviceName", out var sn) ? sn.GetString() ?? "" : "";
        var severity = args.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "" : "";
        var summary = args.TryGetProperty("summary", out var sum) ? sum.GetString() ?? "" : "";

        var request = new CreateIncidentRequest(title, serviceName, severity, summary);
        var (incident, error) = await apiClient.CreateIncidentAsync(request, ct);

        return incident is not null
            ? JsonSerializer.Serialize(new { success = true, incident }, JsonOptions)
            : JsonSerializer.Serialize(new { error = error ?? "Failed to create incident" });
    }

    private async Task<string> ExecuteCloseIncidentAsync(JsonElement args, CancellationToken ct)
    {
        if (!args.TryGetProperty("id", out var idProp))
            return JsonSerializer.Serialize(new { error = "Missing required parameter: id" });

        var id = idProp.GetInt32();
        var resolutionNote = args.TryGetProperty("resolutionNote", out var rn) ? rn.GetString() : null;

        var request = new CloseIncidentRequest(resolutionNote);
        var (incident, error) = await apiClient.CloseIncidentAsync(id, request, ct);

        return incident is not null
            ? JsonSerializer.Serialize(new { success = true, incident }, JsonOptions)
            : JsonSerializer.Serialize(new { error = error ?? $"Failed to close incident {id}" });
    }
}
