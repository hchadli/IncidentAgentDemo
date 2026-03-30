---
name: incident-triage
description: Retrieves and analyzes open incidents for a service, providing severity assessment and prioritization guidance.
---

# Skill: Incident Triage

**Name:** incident-triage  
**Description:** Retrieves and analyzes open incidents for a service, providing severity assessment and prioritization guidance.

## Instructions

When the user asks about incidents for a service:

1. Use `get_open_incidents` with the service name to retrieve current open incidents.
2. If a specific incident ID is mentioned, also use `get_incident_by_id` to get full details.
3. Analyze the results:
   - Count incidents by severity (High / Medium / Low)
   - Identify the most critical incident (highest severity, most recent)
   - Note any patterns (e.g., related incidents, cascading failures)
4. Present findings in a structured format with severity badges.

## Examples

**User:** "Show me open production incidents for Payments"  
**Agent actions:**
- Calls `get_open_incidents(serviceName: "Payments")`
- Receives list of incidents
- Formats response with severity indicators

**User:** "Show me incident 2 and tell me if it's critical"  
**Agent actions:**
- Calls `get_incident_by_id(id: 2)`
- Evaluates severity field
- Provides criticality assessment

**User:** "What are the most urgent incidents right now?"  
**Agent actions:**
- Calls `get_open_incidents()` (no filter — all services)
- Sorts by severity and recency
- Presents top items with context
