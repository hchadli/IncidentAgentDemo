using System.ClientModel;
using IncidentAgentDemo.McpServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Responses;

namespace IncidentAgentDemo.AgentHost;

public sealed class OpenAiAgentClient
{
    private readonly ResponsesClient _responsesClient;
    private readonly ILogger<OpenAiAgentClient> _logger;
    private readonly string _model;

    public OpenAiAgentClient(IConfiguration configuration, ILogger<OpenAiAgentClient> logger)
    {
        _logger = logger;

        var apiKey = configuration["OpenAI:ApiKey"]
                     ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                     ?? throw new InvalidOperationException(
                         "OpenAI API key not found. Set 'OpenAI:ApiKey' via user secrets or the OPENAI_API_KEY environment variable.");

        _model = configuration["OpenAI:Model"] ?? "gpt-4o";

        _logger.LogInformation("[Agent] Using model: {Model}", _model);

        var client = new OpenAIClient(new ApiKeyCredential(apiKey));
        _responsesClient = client.GetResponsesClient();
    }

    public async Task<ResponseResult> CreateResponseAsync(
        CreateResponseOptions options,
        CancellationToken ct = default)
    {
        _logger.LogInformation("[Agent] Sending request to OpenAI Responses API...");
        var result = await _responsesClient.CreateResponseAsync(options, ct);
        _logger.LogInformation("[Agent] Response received. Status: {Status}", result.Value.Status);
        return result.Value;
    }

    public CreateResponseOptions BuildOptions(string? composedInstructions = null)
    {
        var options = new CreateResponseOptions
        {
            Model = _model,
            Instructions = composedInstructions ?? """
                You are an expert incident management assistant for a cloud platform.
                You have access to tools that query real operational data.

                RULES:
                - Always use tools to get incident and service health data. Never make up data.
                - When asked about incidents for a service, use get_open_incidents with the service name.
                - When asked about a specific incident by ID, use get_incident_by_id.
                - When asked about service health, use get_service_health.
                - When asked about risk, use both get_open_incidents AND get_service_health to form a complete picture.
                - Classify risk as: Low (0 high-severity open incidents, healthy status), 
                  Medium (1 high-severity or degraded status), High (2+ high-severity or down status).
                - Format responses clearly with bullet points and severity indicators.
                - If a service name is mentioned, match it exactly: Payments, Identity, or Notifications.
                """
        };

        options.Tools.Add(ResponseTool.CreateFunctionTool(
            ToolDefinitions.GetOpenIncidents,
            BinaryData.FromString(ToolDefinitions.GetOpenIncidentsParameters),
            null,
            ToolDefinitions.GetOpenIncidentsDescription));

        options.Tools.Add(ResponseTool.CreateFunctionTool(
            ToolDefinitions.GetIncidentById,
            BinaryData.FromString(ToolDefinitions.GetIncidentByIdParameters),
            null,
            ToolDefinitions.GetIncidentByIdDescription));

        options.Tools.Add(ResponseTool.CreateFunctionTool(
            ToolDefinitions.GetServiceHealth,
            BinaryData.FromString(ToolDefinitions.GetServiceHealthParameters),
            null,
            ToolDefinitions.GetServiceHealthDescription));

        return options;
    }
}
