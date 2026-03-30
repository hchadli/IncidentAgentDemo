# AGENTS.md — IncidentAgentDemo

## Architecture Overview

IncidentAgentDemo is a multi-project .NET 10 solution that demonstrates a real AI Agent system for cloud incident management. The system follows this flow:

```
User → Blazor UI → AI Agent → MCP Server → API/DB → MCP Server → Agent → UI Response
```

### Projects

| Project | Role |
|---------|------|
| `IncidentAgentDemo.Web` | Blazor Server interactive UI — 3-panel dashboard with chat, agent trace, and quick prompts |
| `IncidentAgentDemo.AgentHost` | Core agent logic — OpenAI Responses API loop, tool dispatch, trace collection |
| `IncidentAgentDemo.McpServer` | Tool definitions and execution — calls the API via HttpClient |
| `IncidentAgentDemo.Api` | ASP.NET Core minimal API with EF Core + SQLite — serves incident and health data |
| `IncidentAgentDemo.Contracts` | Shared DTOs used across all projects |
| `IncidentAgentDemo.Tests` | xUnit integration and unit tests |

### Responsibility Boundaries

- **Agent (AgentHost)**: Decides *when* to call tools, *which* tool to call, *whether* more data is needed, and composes the final response. The agent never fabricates operational data.
- **MCP Server**: Provides tool definitions (name, description, JSON schema) and executes tool calls by invoking the API. It is the bridge between the AI model and the backend.
- **API**: The single source of truth. Serves data from SQLite. Never accessed directly by the agent — always through MCP tools.

---

## Coding Rules

1. Use `async/await` everywhere. No `.Result` or `.Wait()`.
2. Use primary constructors and DI for all services.
3. Use records for DTOs. Use `sealed` classes for services.
4. All public methods must have a `CancellationToken` parameter where applicable.
5. Never throw generic `Exception` — use specific types or return error DTOs.
6. Use `ILogger<T>` for structured logging.

## Logging Rules

1. Every agent decision must be logged: tool selection, arguments, response size, iteration count.
2. Use log level `Information` for normal flow, `Warning` for retries, `Error` for failures.
3. Prefix agent logs with `[Agent]`, MCP logs with `[MCP]`.
4. Never log raw API keys or full API responses in production.

## Tool Design Rules

1. Every tool must have a clear `description` that tells the model when to use it.
2. Parameter schemas must use JSON Schema with descriptions for each parameter.
3. Tools must return structured JSON — never raw text.
4. Tools must handle errors gracefully and return `{ "error": "message" }` on failure.
5. Tool names use `snake_case`: `get_open_incidents`, `get_incident_by_id`, `get_service_health`, `create_incident`, `close_incident`.
6. Mutating tools (`create_incident`, `close_incident`) must return a `success` flag so the model can confirm the outcome.
7. Never claim an incident was created or closed unless the tool response confirms success.

## Skill Usage Rules

1. Skills are documentation for the agent and developers — they describe capabilities.
2. Each skill maps to one or more tools.
3. Skills define *when* to use a capability, *what* data is needed, and *how* to present results.

## Critical Rules

- **Never hallucinate incident data.** Always use tools for operational queries.
- **Always prefer tools** over model knowledge for anything related to incidents, services, or health.
- **Never skip the agent loop.** Even if the model seems confident, it must go through tool execution.
- **Never claim an incident was created** unless `create_incident` returns the new incident with an ID.
- **Never claim an incident was closed** unless `close_incident` returns the updated incident with status Closed.
- When creating incidents, always include service name, severity (Low/Medium/High/Critical), title, and summary.
- When closing incidents, confirm the target incident ID and include the result status in the response.

---

## Run Instructions

### Prerequisites
- .NET 10 SDK
- OpenAI API key

### Setup

```bash
# Set OpenAI API key via user secrets (preferred)
cd src/IncidentAgentDemo.Web
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"

# Or via environment variable
export OPENAI_API_KEY=sk-your-key-here   # Linux/Mac
set OPENAI_API_KEY=sk-your-key-here      # Windows
```

### Run

Terminal 1 — Start the API:
```bash
cd src/IncidentAgentDemo.Api
dotnet run
# Runs on http://localhost:5006
```

Terminal 2 — Start the Blazor UI:
```bash
cd src/IncidentAgentDemo.Web
dotnet run
# Runs on http://localhost:5046
```

Open `http://localhost:5046` in your browser.

### Run Tests
```bash
dotnet test
```

---

## Demo Script

1. Open the Blazor UI at `http://localhost:5046`
2. Click **"💳 Payments Incidents"** — watch the agent trace show tool selection and execution
3. Click **"🔐 Identity Health"** — see the `get_service_health` tool being called
4. Click **"📊 Notifications Risk"** — observe the agent making *two* tool calls (incidents + health)
5. Type: **"Show me incident 2 and tell me if it's critical"** — see `get_incident_by_id` in action
6. Click **"➕ Create Payments Incident"** — watch the agent call `create_incident` and confirm the new ID
7. Click **"✅ Close Incident #5"** — see the agent call `close_incident` and confirm closure
8. Click **"🏗️ Explain Architecture"** in the sidebar to show the flow diagram

### What to Point Out

- The **Agent Trace panel** on the right shows every decision the AI makes
- The agent autonomously decides which tools to call and when it has enough data
- No incident data is ever hallucinated — it all comes from the SQLite database via tools
- The system uses the modern OpenAI Responses API (not legacy chat completions)
- The agent can **create** and **close** incidents via tools — mutations are confirmed, never assumed
- All five tools (`get_open_incidents`, `get_incident_by_id`, `get_service_health`, `create_incident`, `close_incident`) are fully functional
