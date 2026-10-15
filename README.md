# FlowForge AI

**A modular AI workflow orchestration API built with C#, ASP.NET Core, and local LLMs.**

**GitHub:** [DrGnome](https://github.com/DrGnome) · **License:** [MIT](LICENSE)

FlowForge AI is a backend project that turns a user's goal and document into a structured AI-assisted workflow. It combines a modular workflow execution engine with local inference through Ollama, returning analysis results, execution status, and step-level details through a REST API.

The project demonstrates clean architecture, dependency injection, asynchronous programming, cancellation handling, error management, and automated testing in modern .NET.

## Features

- **AI-powered document analysis** — generate a summary, key findings, and caveats from supplied text.
- **Local LLM integration** — run inference through Ollama without relying on a hosted AI API.
- **Modular workflow engine** — execute independently defined workflow steps in sequence.
- **Structured API responses** — return workflow IDs, status, timestamps, analysis, and step results.
- **Failure handling** — stop execution safely when a step fails, with useful application logging.
- **Cancellation support** — handle cancellation before execution and while a step is running.
- **Health endpoint** — check API health and Ollama availability.
- **Workflow history** — retain workflow results in an in-memory repository during the API process lifetime.
- **Automated testing** — cover workflow behavior, AI service requests, and API endpoints.

## Tech Stack

| Technology | Purpose |
|---|---|
| C# | Application development |
| .NET 10 | Runtime and framework |
| ASP.NET Core Minimal APIs | HTTP API |
| Ollama | Local language model inference |
| Llama 3.2 | Default local model |
| xUnit | Automated testing |
| `Microsoft.AspNetCore.Mvc.Testing` | API integration testing |

## Architecture

FlowForge AI uses a layered architecture to separate business logic, workflow orchestration, infrastructure, and HTTP endpoints.

```text
FlowForgeAI/
├── src/
│   ├── FlowForge.Api/
│   │   └── HTTP endpoints and dependency injection
│   ├── FlowForge.Application/
│   │   ├── Workflow orchestration
│   │   ├── Workflow steps
│   │   ├── AI service abstractions
│   │   └── Workflow history contracts
│   ├── FlowForge.Domain/
│   │   └── Core workflow and analysis models
│   └── FlowForge.Infrastructure/
│       ├── Ollama integration
│       ├── AI health checks
│       └── In-memory workflow history
└── tests/
    └── FlowForge.UnitTests/
```

### Design principles

- **Separation of concerns:** domain models, application logic, infrastructure, and API delivery have distinct responsibilities.
- **Dependency inversion:** application code relies on abstractions for AI services and workflow history.
- **Dependency injection:** implementations are registered centrally in the API.
- **Testability:** external AI calls can be replaced with test doubles, allowing deterministic tests without running a model.
- **Resilience:** failures and cancellation are handled explicitly instead of being silently swallowed.

## Getting Started

### Prerequisites

Install:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Ollama](https://ollama.com/)
- Git

### 1. Download the model

Open a terminal and run:

```bash
ollama pull llama3.2
```

Make sure Ollama is running locally. Its default API address is `http://localhost:11434/`.

### 2. Clone the repository

```bash
git clone <YOUR_GITHUB_REPOSITORY_URL>
cd FlowForgeAI
```

Replace the placeholder with your repository URL.

### 3. Configure the AI service

In `src/FlowForge.Api/appsettings.json`, check the AI settings:

```json
{
  "Ai": {
    "BaseUrl": "http://localhost:11434/",
    "Model": "llama3.2"
  }
}
```

### 4. Build and test

From the solution root:

```bash
dotnet restore
dotnet build
dotnet test
```

### 5. Run the API

```bash
dotnet run --project src/FlowForge.Api --urls http://localhost:5080
```

The API will be available at `http://localhost:5080`.

## API Reference

### `GET /health`

Checks API health and whether Ollama is available.

**Example:**

```bash
curl http://localhost:5080/health
```

The endpoint reports a healthy or degraded state and returns an appropriate HTTP status.

### `POST /workflows/run`

Executes a workflow using a goal and source document text.

**Request:**

```json
{
  "goal": "Summarize the document and identify its main risks",
  "documentText": "Paste your source document text here."
}
```

**PowerShell example:**

```powershell
$body = @{
    goal = "Summarize the document and identify its main risks"
    documentText = "Paste your source document text here."
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod `
    -Uri "http://localhost:5080/workflows/run" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

$response | ConvertTo-Json -Depth 10
```

**Response structure:**

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "goal": "Summarize the document and identify its main risks",
  "status": "Succeeded",
  "startedAt": "2026-01-01T10:00:00+00:00",
  "completedAt": "2026-01-01T10:00:05+00:00",
  "analysis": {
    "summary": "A concise summary of the document.",
    "keyFindings": [
      "First key finding",
      "Second key finding"
    ],
    "caveats": [
      "A limitation or uncertainty"
    ]
  },
  "steps": []
}
```

The response above illustrates the shape of the payload; actual IDs, timestamps, findings, and step results will vary.

### `GET /workflows/{id}`

Retrieves a previously saved workflow run by its GUID.

- `200 OK` — workflow run found.
- `404 Not Found` — no matching workflow run exists.

Workflow history is stored in memory and is cleared when the API process restarts.

### Error handling

| Status | Meaning |
|---|---|
| `200 OK` | Workflow completed or resource retrieved successfully |
| `400 Bad Request` | Invalid workflow input |
| `404 Not Found` | Workflow run not found |
| `502 Bad Gateway` | Workflow execution failed |

## Testing

Run the complete test suite with:

```bash
dotnet test
```

The test suite covers areas including:

- Workflow step ordering and failure behavior
- Cancellation before and during execution
- Preservation of AI analysis results
- Ollama request formatting and response validation
- API request validation and error handling
- API health checks

Most tests use fakes or test HTTP handlers, so they do not require a live Ollama instance. The live AI workflow can be tested separately with Ollama running.

## Current Limitations

- Workflow history is in memory and does not survive an API restart.
- The AI service requires Ollama and a locally available model for live inference.
- AI-generated results can be inaccurate or incomplete and should be reviewed for important decisions.
- This is a backend API project; a separate frontend is not included.

## Roadmap

Potential next improvements:

- [ ] Persistent workflow history using SQLite or PostgreSQL
- [ ] More workflow types and configurable step pipelines
- [ ] Retry policies and per-step timeouts
- [ ] Workflow execution metrics and structured tracing
- [ ] OpenAPI documentation and interactive API exploration
- [ ] Containerized deployment with Docker
- [ ] A web-based workflow dashboard

## Why This Project?

FlowForge AI explores how to build a maintainable AI-enabled backend without tightly coupling business logic to a particular language model provider. It combines modern .NET development practices with practical local AI integration and an emphasis on reliability and testability.

## License

This project is licensed under the MIT License.

You are free to use, modify, and distribute this software in accordance with the license terms.