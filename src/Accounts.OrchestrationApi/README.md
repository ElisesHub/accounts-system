# Accounts Orchestration API

The application-facing service of the [accounts system](../../README.md). It sits between the frontend and the [Accounts Data API](../Accounts.DataApi/README.md), and has no direct database access.

```text
Frontend  →  Orchestration API  →  Accounts Data API  →  MySQL
```

## Responsibilities

- Expose account endpoints to the frontend
- Authenticate incoming requests with an API key
- Call the Accounts Data API through a typed `HttpClient`, sending its own API key
- Return account data to the frontend
- Return validation errors in a consistent format
- Handle unexpected exceptions through a global exception handler
- Provide a health check endpoint

## Project structure

```text
src/Accounts.OrchestrationApi/
├── Domain/           Account domain models and business rules
├── Application/      Use-case orchestration and application services
├── Infrastructure/   HTTP client for the Data API, API key validation, configuration
└── Presentation/     Controllers, authentication, error handling, request/response models
```

Requests flow Controller → `IAccountsService` → `IExternalAccountsClient` → Accounts Data API. The typed client keeps the application layer independent of the HTTP integration details:

```csharp
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>();
```

## Technologies

ASP.NET Core Web API (.NET 10), Clean Architecture, Domain-Driven Design, typed `HttpClient`, API key authentication, OpenAPI/Swagger, health checks.

## Authentication

Every controller requires the `RequireApiKey` policy:

```csharp
app.MapControllers().RequireAuthorization("RequireApiKey");
```

Incoming requests must send the key in the `x-api-key` header. Missing or invalid keys are rejected before reaching the controller. `GET /health` is unauthenticated.

## Configuration

| Key | Purpose | Secret |
|---|---|---|
| `AccountsApplicationApiKey` | Key expected from the frontend | Yes |
| `AccountsApiKey` | Key sent to the Accounts Data API | Yes |
| `ExternalDbApi:BaseUrl` | Base URL of the Accounts Data API, as an absolute URI | No |

Startup fails if any value is missing or the base URL is not a valid absolute URI.

**Local development**: the keys are .NET user secrets, and the base URL is in `appsettings.Development.json`:

```bash
cd src/Accounts.OrchestrationApi
dotnet user-secrets set "AccountsApplicationApiKey" "your-accounts-application-api-key"
dotnet user-secrets set "AccountsApiKey" "your-accounts-api-key"
```

```json
{
  "ExternalDbApi": {
    "BaseUrl": "http://localhost:5253"
  }
}
```

**In Docker**: the keys are Docker secrets mounted under `/run/secrets`, and the base URL is set in `docker-compose.yaml` using the compose service name:

```yaml
ExternalDbApi__BaseUrl: "http://data-api:8080"
```

The double underscore maps to the nested key `ExternalDbApi:BaseUrl`.

## Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/health` | None | Service health status |
| GET | `/api/accounts` | API key | Returns all accounts from the Data API |

Swagger UI is enabled in the Development environment only.

## Error responses

Validation errors return:

```json
{
  "code": "ValidationError",
  "message": "One or more validation errors occurred.",
  "fieldErrors": { "fieldName": ["Validation error message"] },
  "traceId": "request-trace-id"
}
```

Unexpected exceptions are handled by `GlobalExceptionHandler` and returned in the same format.

## Running locally

With the Accounts Data API running at the configured base URL:

```bash
dotnet run --project src/Accounts.OrchestrationApi
```

To run the whole system, use Docker Compose from the repository root. See the [root README](../../README.md).

## Troubleshooting

**Startup fails**: check that all three configuration values are set and that the base URL is an absolute URI, such as `http://data-api:8080`.

**401 Unauthorized**: check that the `x-api-key` header matches `AccountsApplicationApiKey`.

**Accounts cannot be retrieved**: check that the Data API is running, that `ExternalDbApi:BaseUrl` points to it, and that `AccountsApiKey` matches the key the Data API expects. A 500 here usually means the error is in the Data API, so check its logs with `docker compose logs data-api`.

**Swagger is not available**: it only runs with `ASPNETCORE_ENVIRONMENT=Development`.