using IncidentAgentDemo.Contracts;
using IncidentAgentDemo.McpServer;
using Microsoft.Extensions.Logging;

namespace IncidentAgentDemo.AgentHost;

public sealed class McpToolExecutor(McpToolRegistry registry, ILogger<McpToolExecutor> logger)
{
    public async Task<string> ExecuteAsync(string toolName, string argumentsJson, CancellationToken ct = default)
    {
        logger.LogInformation("[MCP] Executing tool '{ToolName}' with args: {Args}", toolName, argumentsJson);

        var result = await registry.ExecuteToolAsync(toolName, argumentsJson, ct);

        logger.LogInformation("[MCP] Tool '{ToolName}' returned {Length} chars", toolName, result.Length);
        return result;
    }

    public IReadOnlyList<string> GetAvailableTools() => registry.ToolNames;
}
