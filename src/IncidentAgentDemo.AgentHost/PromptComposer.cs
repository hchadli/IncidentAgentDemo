using System.Text;

namespace IncidentAgentDemo.AgentHost;

public sealed class PromptComposer : IPromptComposer
{
    private const string RuntimeInstructions = """
        You are an expert incident management assistant. Use the available tools to query real operational data.

        Rules:
        - Always use tools to get data. Never fabricate incident or health data.
        - When asked about incidents, use get_open_incidents or get_incident_by_id.
        - When asked about health, use get_service_health.
        - When asked about risk, use both get_open_incidents AND get_service_health.
        - Classify risk as: Low (no high-severity, healthy), Medium (1 high-severity or degraded), High (2+ high-severity or down).
        - Format responses with bullet points and severity indicators.
        - Match service names exactly: Payments, Identity, or Notifications.
        """;

    public string Compose(string? agentsInstructions, IReadOnlyList<SkillDocument> skills)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(agentsInstructions))
        {
            sb.AppendLine("# Global Agent Instructions");
            sb.AppendLine();
            sb.AppendLine(agentsInstructions.Trim());
            sb.AppendLine();
        }

        if (skills.Count > 0)
        {
            sb.AppendLine("# Loaded Skills");
            sb.AppendLine();

            foreach (var skill in skills)
            {
                sb.AppendLine($"## Skill: {skill.Name}");

                if (!string.IsNullOrWhiteSpace(skill.Description))
                    sb.AppendLine($"*{skill.Description}*");

                sb.AppendLine();
                sb.AppendLine(skill.Content.Trim());
                sb.AppendLine();
            }
        }

        sb.AppendLine("# Runtime Instructions");
        sb.AppendLine();
        sb.AppendLine(RuntimeInstructions.Trim());

        return sb.ToString();
    }
}
