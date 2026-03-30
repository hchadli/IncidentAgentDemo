namespace IncidentAgentDemo.Contracts;

public sealed record OpenIncidentsResponse(
    string ServiceName,
    int Count,
    IReadOnlyList<IncidentDto> Incidents);
