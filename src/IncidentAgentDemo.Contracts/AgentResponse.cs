namespace IncidentAgentDemo.Contracts;

public sealed class AgentResponse
{
    public required string FinalAnswer { get; init; }
    public required IReadOnlyList<AgentTraceStep> TraceSteps { get; init; }
    public bool IsError { get; init; }
}
