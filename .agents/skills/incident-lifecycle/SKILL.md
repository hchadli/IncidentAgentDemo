---
name: incident-lifecycle
description: Guides the agent on creating new incidents and closing existing incidents through MCP tools.
---

# Skill: Incident Lifecycle

**Name:** incident-lifecycle  
**Description:** Guides the agent on creating new incidents and closing existing incidents through MCP tools.

## Instructions

### Creating Incidents

When the user asks to create, open, or report a new incident:

1. Extract the following from the user's message:
   - **title** — a concise description of the problem
   - **serviceName** — the affected service (Payments, Identity, or Notifications)
   - **severity** — one of: Low, Medium, High, Critical
   - **summary** — a detailed explanation of the issue
2. If any required field is missing or ambiguous, ask the user for clarification before calling the tool.
3. Call `create_incident` with all four arguments.
4. On success, confirm the creation and include the assigned incident ID, severity, and service name.
5. On failure, report the error message from the tool response — never claim the incident was created.

### Closing Incidents

When the user asks to close, resolve, or complete an incident:

1. Identify the **incident ID** from the user's message.
2. Optionally extract a **resolution note** if the user provides one.
3. Call `close_incident` with the id and optional resolutionNote.
4. On success, confirm the closure and include the incident ID and final status.
5. On failure (e.g., incident not found or already closed), report the exact error — never claim the incident was closed.

### Validation

- Severity must be one of: Low, Medium, High, Critical.
- Service names must match existing services in the system.
- An incident that is already closed cannot be closed again.

## Examples

**User:** "Create a high severity incident for Payments about failed transactions"  
**Agent actions:**
- Calls `create_incident(title: "Failed transactions in Payments", serviceName: "Payments", severity: "High", summary: "Multiple payment transactions are failing with timeout errors across the EU region.")`
- Returns: "✅ Incident #10 created — High severity for Payments."

**User:** "Open an incident for Identity because login is failing intermittently"  
**Agent actions:**
- Calls `create_incident(title: "Intermittent login failures", serviceName: "Identity", severity: "Medium", summary: "Users are reporting intermittent login failures. The issue appears to be sporadic and affects multiple regions.")`
- Returns: "✅ Incident #11 created — Medium severity for Identity."

**User:** "Close incident 3"  
**Agent actions:**
- Calls `close_incident(id: 3)`
- Returns: "✅ Incident #3 has been closed."

**User:** "Resolve incident 5 with note: issue fixed after config rollback"  
**Agent actions:**
- Calls `close_incident(id: 5, resolutionNote: "Issue fixed after config rollback")`
- Returns: "✅ Incident #5 has been closed. Resolution: issue fixed after config rollback."

**User:** "Close incident 999"  
**Agent actions:**
- Calls `close_incident(id: 999)`
- Tool returns error: incident not found
- Returns: "❌ Could not close incident 999 — incident not found."
