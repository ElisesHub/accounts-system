# Accounts Orchestration API

The application-facing service of the [accounts system](../../README.md). It exposes a REST API to the frontend and calls the [Accounts Data API](../Accounts.DataApi/README.md) over gRPC. It has no direct database access.

```text
Frontend  →  HTTP + API key  →  Orchestration API  →  gRPC over TLS + JWT  →  Accounts Data API  →  PostgreSQL
```

## Responsibilities

- Expose account endpoints to the frontend as a REST API
- Authenticate incoming requests with an API key
- Call the Accounts Data API through a gRPC client, over TLS, with an Auth0 access token
- Translate between the REST models the frontend uses and the gRPC contract of the Data API
- Handle unexpected exceptions through a global exception handler
- Expose a liveness health check

## Project structure

```text
src/Accounts.OrchestrationApi/
├── OrchestrationApi/
│   ├── Domain/           Account domain models and business rules
│   ├── Application/      Use-case orchestration and application services
│   ├── Infrastructure/   gRPC client of the Data API, Auth0 token provider, API key validation, configuration
│   └── Presentation/     Controllers, authentication, error handling, request/response models
└── OrchestrationApi.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

Requests flow controller → application service → gRPC client → Accounts Data API. The application layer depends on an interface, so it knows nothing about gRPC, TLS or tokens.

## Technologies

ASP.NET Core Web API (.NET 10), Clean Architecture, Domain-Driven Design, gRPC client (`Grpc.Net.ClientFactory`), Protocol Buffers, Auth0 client credentials, TLS with a private CA, API key authentication, OpenAPI/Swagger, ASP.NET Core health checks, structured JSON logging, xUnit v3, AwesomeAssertions.

## Authentication

**Incoming requests.** The API is secure by default: its fallback authorization policy requires the API key scheme on every endpoint. Requests must send the key in the `x-api-key` header, and missing or invalid keys are rejected before reaching the controller. The health endpoint opts out explicitly with `AllowAnonymous()`.

**Calls to the Data API.** The gRPC client attaches an Auth0 access token to every call as call credentials. The token is obtained with the client credentials flow, using the Orchestration API's machine-to-machine application in Auth0. The Data API requires the `read:accounts` scope.

## TLS to the Data API

The gRPC client connects to the Data API over HTTPS. Instead of the operating system's trust store, it trusts only the private CA that issued the Data API's certificate, loaded from the configured CA certificate file. Certificate validation is never disabled. If no CA certificate path is configured, the client falls back to the operating system's trust store.

See [TLS between services](../../README.md#tls-between-services) for creating the certificates.

## Configuration

Required settings are bound to options classes and validated at startup. The API fails fast if a required value is missing, if the Data API base URL is not a valid absolute URL, or if a CA certificate path is set but the file doesn't exist.

| Setting | Purpose | Secret |
|---|---|---|
| `AccountsApplicationApiKey` | Key expected from the frontend | Yes |
| Auth0 client ID and secret | Credentials of the machine-to-machine application used to get access tokens | Yes |
| Data API base URL | Address of the Data API, e.g. `https://data-api:8081` in Docker | No |
| CA certificate path | Root certificate used to trust the Data API | No |

**Local development**: secrets are .NET user secrets, and non-secret local values are in `appsettings.Development.json`.

**In Docker**: secrets are mounted under `/run/secrets`, and per-deployment values such as the base URL and the CA certificate path are set as environment variables in `docker-compose.yaml`. A double underscore in an environment variable name maps to a nested configuration key (`Section__Key` → `Section:Key`).

## Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/health/live` | None | The process is running; no dependency checks |
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

Unexpected exceptions, including failed calls to the Data API, are handled by `GlobalExceptionHandler`, logged once with the W3C trace ID, and returned in the same format.

## Logging

Outside the Development environment, the API writes JSON console logs that include trace and span IDs. In Development, it uses the simple console format.

## Running locally

With the Accounts Data API running at the configured base URL:

```bash
dotnet run --project src/Accounts.OrchestrationApi/OrchestrationApi
```

To run the whole system, use Docker Compose from the repository root. See the [root README](../../README.md).

## Tests

```bash
dotnet test src/Accounts.OrchestrationApi/OrchestrationApi.Tests
```

Unit tests cover API key validation. Integration tests run the API in memory with test configuration and check, for example, that the health endpoint is reachable without authentication.

## Troubleshooting

**Startup fails**: check that every required setting is set, that the Data API base URL is an absolute URL such as `https://data-api:8081`, and that the CA certificate file exists at the configured path.

**401 Unauthorized**: check that the `x-api-key` header matches `AccountsApplicationApiKey`.

**Certificate or SSL errors when calling the Data API**: the CA certificate is missing, or it comes from a different CA than the one that issued the Data API's certificate.

**`Unauthenticated` or `PermissionDenied` from the Data API**: the Auth0 credentials are wrong, or the machine-to-machine application hasn't been granted `read:accounts`.

**Accounts cannot be retrieved**: check that the Data API is running and that the base URL points to it. A 500 here usually means the error is in the Data API, so check its logs with `docker compose logs data-api`.

**Swagger is not available**: it only runs with `ASPNETCORE_ENVIRONMENT=Development`.