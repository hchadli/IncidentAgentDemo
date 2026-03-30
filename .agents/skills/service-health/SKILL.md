---
name: service-health
description: Checks and reports the current health status of platform services.
---

# Skill: Service Health

**Name:** service-health  
**Description:** Checks and reports the current health status of platform services.

## Instructions

When the user asks about service health:

1. Use `get_service_health` with the service name to retrieve current status.
2. Interpret the status:
   - **Healthy**: Service is operating normally. Mention any minor notes.
   - **Degraded**: Service has issues but is partially operational. Highlight the notes for context.
   - **Down**: Service is non-operational. This is critical — emphasize urgency.
3. Include the last check timestamp to show data freshness.
4. If relevant, suggest checking open incidents for more context.

## Examples

**User:** "What is the health of the Identity service?"  
**Agent actions:**
- Calls `get_service_health(serviceName: "Identity")`
- Receives health record
- Formats response with health badge and notes

**User:** "Is Payments up?"  
**Agent actions:**
- Calls `get_service_health(serviceName: "Payments")`
- Reports Degraded status with context from notes

**User:** "Check all services"  
**Agent actions:**
- Calls `get_service_health` for each known service (Payments, Identity, Notifications)
- Presents a summary table
