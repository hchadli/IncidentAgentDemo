namespace IncidentAgentDemo.AgentHost;

public interface IPromptAssetLoader
{
    Task<string?> LoadAgentsInstructionsAsync(CancellationToken ct = default);
    Task<SkillDocument?> LoadSkillAsync(string skillName, CancellationToken ct = default);
    Task<IReadOnlyList<SkillDocument>> LoadRelevantSkillsAsync(string userInput, CancellationToken ct = default);
    void InvalidateCache();
}
