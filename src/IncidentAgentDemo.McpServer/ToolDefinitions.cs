namespace IncidentAgentDemo.McpServer;

public static class ToolDefinitions
{
    public const string GetOpenIncidents = "get_open_incidents";
    public const string GetIncidentById = "get_incident_by_id";
    public const string GetServiceHealth = "get_service_health";

    public static readonly string GetOpenIncidentsDescription =
        "Retrieves all currently open incidents, optionally filtered by service name. " +
        "Returns incident count and details including title, severity, status, and summary.";

    public static readonly string GetIncidentByIdDescription =
        "Retrieves a single incident by its numeric ID. " +
        "Returns full incident details including title, service, severity, status, created date, and summary.";

    public static readonly string GetServiceHealthDescription =
        "Retrieves the current health status of a specific service. " +
        "Returns service status (Healthy/Degraded/Down), last check time, and notes.";

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
}
