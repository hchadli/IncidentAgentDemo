namespace IncidentAgentDemo.Contracts;

public sealed class AgentTraceStep
{
    public required string Message { get; init; }
    public required string StepType { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
