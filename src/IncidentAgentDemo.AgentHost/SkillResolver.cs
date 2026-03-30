namespace IncidentAgentDemo.AgentHost;

public sealed class SkillResolver : ISkillResolver
{
    private static readonly Dictionary<string, string[]> SkillKeywords = new()
    {
        ["incident-triage"] = ["incident", "incidents", "severity", "urgent", "critical", "sev1", "sev2"],
        ["incident-lifecycle"] = ["create", "open", "close", "resolve", "resolved", "new incident", "report incident"],
        ["service-health"] = ["health", "status", "healthy", "degraded", "down"],
        ["risk-summary"] = ["risk", "summary", "impact", "assessment"],
        ["demo-runbook"] = ["architecture", "demo", "explain", "runbook", "walkthrough"]
    };

    public IReadOnlyList<string> ResolveSkills(string userInput)
    {
        var input = userInput.ToLowerInvariant();
        var matched = new List<string>();

        foreach (var (skill, keywords) in SkillKeywords)
        {
            if (keywords.Any(kw => input.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                matched.Add(skill);
        }

        return matched;
    }
}
