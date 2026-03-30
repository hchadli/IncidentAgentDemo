namespace IncidentAgentDemo.Contracts;

public sealed record CreateIncidentRequest(
    string Title,
    string ServiceName,
    string Severity,
    string Summary);
