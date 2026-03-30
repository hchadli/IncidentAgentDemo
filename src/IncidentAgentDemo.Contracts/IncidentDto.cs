namespace IncidentAgentDemo.Contracts;

public sealed record IncidentDto(
    int Id,
    string Title,
    string ServiceName,
    string Severity,
    string Status,
    DateTime CreatedAtUtc,
    string Summary,
    DateTime? ClosedAtUtc = null,
    string? ResolutionNote = null);
