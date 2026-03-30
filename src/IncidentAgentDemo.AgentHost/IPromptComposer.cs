namespace IncidentAgentDemo.AgentHost;

public interface IPromptComposer
{
    string Compose(string? agentsInstructions, IReadOnlyList<SkillDocument> skills);
}
