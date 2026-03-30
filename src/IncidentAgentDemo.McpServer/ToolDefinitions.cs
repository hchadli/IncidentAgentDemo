namespace IncidentAgentDemo.McpServer;

public static class ToolDefinitions
{
    public const string GetOpenIncidents = "get_open_incidents";
    public const string GetIncidentById = "get_incident_by_id";
    public const string GetServiceHealth = "get_service_health";
    public const string CreateIncident = "create_incident";
    public const string CloseIncident = "close_incident";

    public static readonly string GetOpenIncidentsDescription =
        "Retrieves all currently open incidents, optionally filtered by service name. " +
        "Returns incident count and details including title, severity, status, and summary.";

    public static readonly string GetIncidentByIdDescription =
        "Retrieves a single incident by its numeric ID. " +
        "Returns full incident details including title, service, severity, status, created date, and summary.";

    public static readonly string GetServiceHealthDescription =
        "Retrieves the current health status of a specific service. " +
        "Returns service status (Healthy/Degraded/Down), last check time, and notes.";

    public static readonly string CreateIncidentDescription =
        "Creates a new incident in the system. Use this when the user asks to open, create, or report a new incident. " +
        "Requires a title, service name (Payments, Identity, or Notifications), severity (Low, Medium, High, or Critical), and a summary. " +
        "Returns the newly created incident with its assigned ID.";

    public static readonly string CloseIncidentDescription =
        "Closes an existing open incident by its numeric ID. Use this when the user asks to close or resolve an incident. " +
        "Optionally accepts a resolution note describing how the issue was resolved. " +
        "Returns the updated incident. Fails if the incident does not exist or is already closed.";

    public static readonly string GetOpenIncidentsParameters = """
        {
            "type": "object",
            "properties": {
                "serviceName": {
                    "type": "string",
                    "description": "The name of the service to filter incidents for (e.g. Payments, Identity, Notifications). If omitted, returns all open incidents."
                }
            },
            "required": []
        }
        """;

    public static readonly string GetIncidentByIdParameters = """
        {
            "type": "object",
            "properties": {
                "id": {
                    "type": "integer",
                    "description": "The unique numeric ID of the incident to retrieve."
                }
            },
            "required": ["id"]
        }
        """;

    public static readonly string GetServiceHealthParameters = """
        {
            "type": "object",
            "properties": {
                "serviceName": {
                    "type": "string",
                    "description": "The name of the service to check health for (e.g. Payments, Identity, Notifications)."
                }
            },
            "required": ["serviceName"]
        }
        """;

    public static readonly string CreateIncidentParameters = """
        {
            "type": "object",
            "properties": {
                "title": {
                    "type": "string",
                    "description": "A concise title describing the incident (e.g. 'Payment gateway timeout in EU region')."
                },
                "serviceName": {
                    "type": "string",
                    "description": "The affected service name. Must be one of: Payments, Identity, Notifications."
                },
                "severity": {
                    "type": "string",
                    "description": "The severity level. Must be one of: Low, Medium, High, Critical."
                },
                "summary": {
                    "type": "string",
                    "description": "A detailed summary of the incident including symptoms, impact, and any known context."
                }
            },
            "required": ["title", "serviceName", "severity", "summary"]
        }
        """;

    public static readonly string CloseIncidentParameters = """
        {
            "type": "object",
            "properties": {
                "id": {
                    "type": "integer",
                    "description": "The unique numeric ID of the incident to close."
                },
                "resolutionNote": {
                    "type": "string",
                    "description": "Optional note describing how the incident was resolved."
                }
            },
            "required": ["id"]
        }
        """;
}
