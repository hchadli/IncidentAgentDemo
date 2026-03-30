namespace IncidentAgentDemo.AgentHost;

public interface ISkillResolver
{
    IReadOnlyList<string> ResolveSkills(string userInput);
}
