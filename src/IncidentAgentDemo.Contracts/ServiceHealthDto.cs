namespace IncidentAgentDemo.Contracts;

public sealed record ServiceHealthDto(
    int Id,
    string ServiceName,
    string Status,
    DateTime LastCheckedAtUtc,
    string Notes);
