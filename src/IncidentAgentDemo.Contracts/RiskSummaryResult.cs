namespace IncidentAgentDemo.Contracts;

public sealed record RiskSummaryResult(
    string ServiceName,
    string RiskLevel,
    int OpenIncidentCount,
    int HighSeverityCount,
    string HealthStatus,
    string Summary);
