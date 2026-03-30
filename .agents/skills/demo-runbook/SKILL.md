# Skill: Demo Runbook

**Name:** demo-runbook  
**Description:** Step-by-step guide for running the IncidentAgentDemo for presentations and walkthroughs.

## Instructions

Follow this runbook to demonstrate the AI Agent system:

### Pre-Demo Setup

1. Ensure .NET 10 SDK is installed.
2. Set the OpenAI API key:
   ```bash
   cd src/IncidentAgentDemo.Web
   dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"
   ```
3. Start the API in one terminal:
   ```bash
   cd src/IncidentAgentDemo.Api
   dotnet run
   ```
4. Start the Blazor UI in another terminal:
   ```bash
   cd src/IncidentAgentDemo.Web
   dotnet run
   ```
5. Open `http://localhost:5046` in a browser.

### Demo Flow

**Step 1 — Show the Architecture**
- Click "🏗️ Explain Architecture" in the sidebar
- Walk through the flow: User → Agent → MCP → API → DB

**Step 2 — Basic Query**
- Click "💳 Payments Incidents"
- Point out the Agent Trace panel showing each decision step
- Highlight: "Agent decided tool is needed" → "Tool selected" → "Tool response received"

**Step 3 — Service Health**
- Click "🔐 Identity Health"
- Show how the agent selects a different tool based on the query

**Step 4 — Multi-Tool Query (Risk)**
- Click "📊 Notifications Risk"
- Watch the trace: the agent makes TWO tool calls autonomously
- Explain: the model decided it needed both incidents AND health data

**Step 5 — Free-Form Query**
- Type: "Show me incident 2 and tell me if it's critical"
- Show the agent selecting `get_incident_by_id` and reasoning about criticality

**Step 6 — Complex Query**
- Type: "Show me all open incidents across all services and rank them by severity"
- Watch the agent handle a broader query

### Key Talking Points

- The agent decides autonomously which tools to use — no hardcoded routing
- All data comes from the database via tools — never hallucinated
- The trace panel provides full observability into agent reasoning
- The system uses the modern OpenAI Responses API
- The architecture separates concerns: UI / Agent / Tools / Data

## Examples

**Good demo prompts:**
- "Show me open production incidents for Payments"
- "What is the health of the Identity service?"
- "Summarise the risk for Notifications"
- "Show me incident 2 and tell me if it's critical"
- "Which service has the most critical issues right now?"
- "Are there any incidents related to duplicate charges?"
