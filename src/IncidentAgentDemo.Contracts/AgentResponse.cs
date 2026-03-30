namespace IncidentAgentDemo.Contracts;

public sealed class AgentResponse
{
    public required string FinalAnswer { get; init; }
    public required IReadOnlyList<AgentTraceStep> TraceSteps { get; init; }
    public bool IsError { get; init; }
    public bool UsedAgentsFile { get; init; }
    public IReadOnlyList<string> LoadedSkills { get; init; } = [];
    public IReadOnlyList<string> PromptAssetWarnings { get; init; } = [];
    public string? ComposedPrompt { get; init; }
}
