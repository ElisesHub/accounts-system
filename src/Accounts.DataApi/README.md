# Accounts Data API

The database-facing service of the [accounts system](../../README.md). It serves gRPC to the Orchestration API and is the only component that connects to PostgreSQL.

```text
Orchestration API  →  gRPC over TLS + JWT  →  Accounts Data API  →  PostgreSQL 18
```

## Responsibilities

- Serve the `accounts.v1.AccountsService` and `statements.v1.StatementsService` gRPC contracts
- Authenticate callers with JWT access tokens from Auth0 and require the `read:accounts` scope
- Run read-only queries against PostgreSQL as the `accounts_app` role
- Translate expected outcomes into gRPC status codes in one place
- Expose liveness and readiness health checks

## Project structure

```text
src/Accounts.DataApi/
├── AccountsAPI/
│   ├── Domain/           Account domain models and business rules
│   ├── Application/      Service interfaces and application services
│   ├── Infrastructure/   PostgreSQL repositories and configuration
│   └── Presentation/     gRPC service implementations and error mapping
└── AccountsAPI.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

Requests flow gRPC service → application service → repository → PostgreSQL. The gRPC services contain no data-access logic, and the repositories know nothing about gRPC.

## Technologies

ASP.NET Core gRPC (.NET 10, `Grpc.AspNetCore`), Protocol Buffers, Clean Architecture, Domain-Driven Design, PostgreSQL 18 with Npgsql, JWT bearer authentication (Auth0), TLS, ASP.NET Core health checks, structured JSON logging, xUnit v3, Testcontainers, NSubstitute, AwesomeAssertions.

## gRPC services

| Service | RPC | Kind |
|---|---|---|
| `accounts.v1.AccountsService` | `GetAccount` | Unary |
| `accounts.v1.AccountsService` | `GetAccounts` | Unary |
| `statements.v1.StatementsService` | `GetTransactions` | Server streaming |

The contracts live in [`protos/`](../../protos) at the repository root.

`GetTransactions` streams one message per transaction, in date order, reading rows from PostgreSQL and writing them to the response one at a time. If the client disconnects or its deadline expires, cancellation flows back to the database query. The service checks that the account exists before streaming, so a missing account returns `NotFound` rather than an empty stream.

## Endpoints and ports

| Port | Protocol | Serves |
|---|---|---|
| 8081 | HTTPS, HTTP/2 | gRPC |
| 8080 | HTTP/1.1 | Health checks |

| Path | Auth | Checks |
|---|---|---|
| `/health/live` | None | The process is running; no dependency checks |
| `/health/ready` | None | The database is reachable |

## Authentication and authorization

The API is secure by default: a fallback authorization policy requires an authenticated caller on every endpoint. Each gRPC service also requires the `read:accounts` scope, so a future write service can require its own scope. The health endpoints opt out explicitly with `AllowAnonymous()`.

Callers send an Auth0 access token in the `Authorization: Bearer` header. The Orchestration API obtains it with the client credentials flow.

## TLS

In Docker, Kestrel serves gRPC on port 8081 with a certificate issued by the system's private CA. The certificate and key paths are set by Compose environment variables and point to Docker secrets under `/run/secrets`. When the API runs outside Docker, it uses the .NET development certificate instead. See [TLS between services](../../README.md#tls-between-services) for creating the certificate.

## Configuration

Required settings are bound to options classes and validated at startup, so the API fails fast if one is missing or invalid. Values that are the same everywhere live in `appsettings.json`, values that differ per deployment come from environment variables, and secrets come from secret files.

The database password is the only secret part of the connection: host, database name and username are ordinary configuration.

**Local development** uses .NET user secrets for secret values:

```bash
cd src/Accounts.DataApi/AccountsAPI
dotnet user-secrets list
```

**In Docker**, secrets are mounted under `/run/secrets` and read by the key-per-file configuration provider. See the [root README](../../README.md) for setting up the `secrets/` folder.

## Database

The API connects as `accounts_app`, a read-only role created by the database init scripts in `DbFiles/init/`. It uses inline SQL in its repositories rather than stored procedures or database functions, so every query lives next to the code that uses it. See [Database](../../README.md#database) in the root README for the schema and roles.

## Errors

Expected outcomes, such as an account not found or an invalid ID, are returned as results and translated to gRPC status codes (`NotFound`, `InvalidArgument`) in one place. Unexpected faults are logged once on the server with the W3C trace ID, and the caller receives a generic error message.

## Logging

Outside the Development environment, the API writes JSON console logs that include trace and span IDs. In Development, it uses the simple console format.

## Running locally

With a PostgreSQL instance reachable through your configured connection settings:

```bash
dotnet run --project src/Accounts.DataApi/AccountsAPI
```

To run the whole system, use Docker Compose from the repository root. See the [root README](../../README.md).

## Tests

```bash
dotnet test src/Accounts.DataApi/AccountsAPI.Tests
```

Docker must be running. The integration tests use Testcontainers to start a disposable PostgreSQL 18 container, initialised with the same scripts in `DbFiles/init/` as Docker Compose. The API then runs in memory, and the tests call it through a real gRPC client, including the streaming RPC. They also check that the health endpoints are reachable without authentication.

## Troubleshooting

**`Unauthenticated`**: the call has no access token, or the token is invalid or has the wrong audience.

**`PermissionDenied`**: the token is valid but lacks the `read:accounts` scope. Grant it to the machine-to-machine application in Auth0.

**`/health/ready` fails**: the API can't reach PostgreSQL. Check the host (`db` in Docker), the `accounts_app` password, and that the database has finished initialising.

**`password authentication failed for user "accounts_app"`**: the database was initialised with a different password. Reset the volume from the repository root with `docker compose down -v`.