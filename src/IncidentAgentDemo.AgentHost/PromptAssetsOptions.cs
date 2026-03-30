namespace IncidentAgentDemo.AgentHost;

public sealed class PromptAssetsOptions
{
    public string AgentsFilePath { get; set; } = "AGENTS.md";
    public string SkillsRootPath { get; set; } = ".agents/skills";
    public bool EnableSkills { get; set; } = true;
}
