---
name: risk-summary
description: Combines incident data and service health to produce a risk assessment for a service.
---

# Skill: Risk Summary

**Name:** risk-summary  
**Description:** Combines incident data and service health to produce a risk assessment for a service.

## Instructions

When the user asks about risk for a service:

1. Use **both** tools:
   - `get_open_incidents(serviceName)` — to count incidents and severity distribution
   - `get_service_health(serviceName)` — to get current health status
2. Classify risk using this matrix:
   - **Low**: 0 high-severity open incidents AND Healthy status
   - **Medium**: 1 high-severity incident OR Degraded status
   - **High**: 2+ high-severity incidents OR Down status
3. Compose a summary that includes:
   - Risk level with justification
   - Open incident count and severity breakdown
   - Current health status
   - Recommended actions

## Examples

**User:** "Summarise the risk for Notifications"  
**Agent actions:**
- Calls `get_open_incidents(serviceName: "Notifications")`
- Calls `get_service_health(serviceName: "Notifications")`
- Counts high-severity incidents, checks health status
- Classifies as High/Medium/Low
- Presents structured risk summary

**User:** "What's the overall risk posture?"  
**Agent actions:**
- Calls both tools for each service (Payments, Identity, Notifications)
- Produces per-service risk assessment
- Highlights highest-risk service
