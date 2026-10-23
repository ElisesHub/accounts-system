# Accounts Data API

The database-facing service of the [accounts system](../../README.md). It is the only component that connects to MySQL; the Orchestration API calls it over HTTP with an API key.

```text
Orchestration API  →  Accounts Data API  →  MySQL
```

## Responsibilities

- Expose account endpoints to the Orchestration API
- Authenticate requests with an API key
- Run account queries against the MySQL `AccountsDb` database
- Return validation errors in a consistent format
- Handle unexpected exceptions through a global exception handler
- Provide a database-aware health check

## Project structure

```text
src/Accounts.DataApi/
├── AccountsAPI/
│   ├── Domain/           Account domain models and business rules
│   ├── Application/      Service interfaces and application services
│   ├── Infrastructure/   MySQL repositories, API key validation, configuration
│   └── Presentation/     Controllers, authentication, error handling, request/response models
└── AccountsAPI.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

Requests flow Controller → `IAccountsService` → `IAccountsRepository` → MySQL, keeping controllers free of application and data-access logic.

## Technologies

ASP.NET Core Web API (.NET 10), Clean Architecture, Domain-Driven Design, MySQL with MySqlConnector, API key authentication, OpenAPI/Swagger, health checks, xUnit v3, Moq, FluentAssertions.

## Authentication

Every controller requires the `RequireApiKey` policy:

```csharp
app.MapControllers().RequireAuthorization("RequireApiKey");
```

Requests must send the key in the `x-api-key` header. Missing or invalid keys are rejected before reaching the controller. `GET /health` is unauthenticated so Docker can check container health.

## Configuration

| Key | Purpose |
|---|---|
| `ConnectionStrings:AccountsDb` | MySQL connection string |
| `AccountsApiKey` | Key expected from the Orchestration API |

The API fails at startup if either is missing.

**Local development** uses .NET user secrets:

```bash
cd src/Accounts.DataApi/AccountsAPI
dotnet user-secrets set "ConnectionStrings:AccountsDb" "Server=localhost;Port=3306;Database=AccountsDb;Uid=accounts_user;Pwd=yourpassword;"
dotnet user-secrets set "AccountsApiKey" "your-accounts-api-key"
```

**In Docker**, both values are Docker secrets mounted under `/run/secrets` and read by the key-per-file configuration provider. The connection string secret is mounted with the target name `ConnectionStrings:AccountsDb`, so the code reads it exactly as it does locally:

```csharp
builder.Configuration.GetConnectionString("AccountsDb")
```

See the [root README](../../README.md) for setting up the `secrets/` folder.

## Database

The API expects the `AccountsDb` schema, including its stored procedures, to exist. In Docker it is created from `mySQLDb/accountsdb_dump_v1.sql` the first time the database volume is created.

## Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/health` | None | Checks the API can reach the database |
| GET | `/api/accounts` | API key | Returns all accounts |

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

With a MySQL instance reachable through your user-secrets connection string:

```bash
dotnet run --project src/Accounts.DataApi/AccountsAPI
```

To run the whole system, use Docker Compose from the repository root. See the [root README](../../README.md).

## Tests

```bash
dotnet test src/Accounts.DataApi/AccountsAPI.Tests
```

Unit tests cover the application services and API key validation. Integration tests run the repository against a real MySQL database.

## Troubleshooting

**`PROCEDURE AccountsDb.GetAccounts does not exist`**: the database was not initialised from the dump. Recreate the volume from the repository root with `docker compose down -v`, then `docker compose up --build`.

**401 Unauthorized**: check the `x-api-key` header matches the configured `AccountsApiKey`.

**Health check fails**: check the connection string's host (`db` in Docker, `localhost` when running locally), credentials, and that MySQL has finished initialising.