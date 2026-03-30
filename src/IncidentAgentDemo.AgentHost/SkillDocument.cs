namespace IncidentAgentDemo.AgentHost;

public sealed record SkillDocument
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string FilePath { get; init; }
    public required string Content { get; init; }
}
