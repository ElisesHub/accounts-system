# Accounts System

A multi-service accounts application built with ASP.NET Core and run with Docker Compose. It is a deliberately small prototype, used to demonstrate service separation, gRPC between services, a server-streaming RPC in the Data API, API key authentication, secrets management and containerised deployment.

## Architecture

```mermaid
flowchart LR
    Browser -->|HTTP| Web[Accounts Web<br/>Razor Pages / MVC]
    Web -->|HTTP + API key| Orch[Orchestration API]
    Orch -->|gRPC + API key| Data[Accounts Data API]
    Data -->|Npgsql| DB[(PostgreSQL 18)]
```

Only the frontend is exposed outside Docker. Each API authenticates the service calling it with its own API key, and only the Data API can reach the database.

| Service | Compose name | Role | Docs |
|---|---|---|---|
| Accounts Web | `web` | User interface; Auth0 sign-in configured but not yet required | [README](src/Accounts.Web/README.md) |
| Orchestration API | `orchestration-api` | Application-facing REST API; gRPC client of the Data API | [README](src/Accounts.OrchestrationApi/README.md) |
| Accounts Data API | `data-api` | Serves gRPC (plus a `/health` endpoint); the only service with database access | [README](src/Accounts.DataApi/README.md) |
| PostgreSQL | `db` | Accounts database | — |

## Why this architecture

This application doesn't need three tiers. At its size, a Blazor Server or MVC app with a service layer talking directly to the database would be the right design.

It is split into services on purpose, to demonstrate service-to-service communication: a shared Protocol Buffers contract, unary gRPC calls between the APIs, a server-streaming RPC, deadlines and cancellation across a network boundary, and mapping errors between services. In a real system, a separate internal data service starts to pay off when several consumers share the same data, when different teams own different services, or when services need to be deployed and scaled independently.

The Orchestration API is the only service the frontend talks to. It translates between the REST API the frontend uses and the gRPC contract of the Data API, and it is the only service that can call the Data API.

## Technologies

.NET 10, ASP.NET Core, Razor Pages, MVC, REST APIs, gRPC (`Grpc.AspNetCore`, `Grpc.Net.ClientFactory`), Protocol Buffers, PostgreSQL 18, Npgsql, Clean Architecture, Domain-Driven Design, Auth0, Docker Compose with Docker secrets, xUnit v3, Testcontainers.

## Repository structure

```text
accounts-system/
├── src/
│   ├── Accounts.Web/                 Frontend
│   ├── Accounts.OrchestrationApi/    Orchestration API (REST in, gRPC out)
│   └── Accounts.DataApi/             Data API (gRPC) and its tests
├── protos/
│   ├── accounts/v1/                  accounts.v1.AccountsService contract
│   └── statements/v1/                statements.v1.StatementsService contract
├── DbFiles/init/                     Database initialisation scripts
├── secrets.example/                  Template for the secrets/ folder
├── docker-compose.yaml
└── AccountsSystem.slnx
```

## Quick start

**Prerequisites:** Docker Desktop (or Docker Engine with Compose v2).

**1. Clone the repository**

```bash
git clone https://github.com/ElisesHub/accounts-system.git
cd accounts-system
```

**2. Create the secrets folder from the template**

```bash
cp -r secrets.example secrets
```

Replace every placeholder in `secrets/` with a real value. Each file contains only the value, with no quotes and **no trailing newline**. The safest way to write one is with `printf`:

```bash
printf '%s' 'your-value-here' > secrets/DbAppPassword.txt
```

| File | Used by | Value |
|---|---|---|
| `AccountsApiKey.txt` | Orchestration API → Data API | Any strong random string |
| `AccountsApplicationApiKey.txt` | Web → Orchestration API | Any strong random string |
| `Auth0ClientId.txt` | Web | Leave the placeholder value |
| `Auth0ClientSecret.txt` | Web | Leave the placeholder value |
| `DbAdminPassword.txt` | PostgreSQL | Password for the `accounts_admin` superuser (database setup only) |
| `DbAppPassword.txt` | PostgreSQL and Data API | Password for the `accounts_app` role the Data API connects as |

`secrets/` is ignored by git. Never commit it.

**3. Build and start**

```bash
docker compose up --build
```

Open **http://localhost:1000**.

## Everyday commands

| Task | Command |
|---|---|
| Start in the background | `docker compose up --build -d` |
| Follow all logs | `docker compose logs -f` |
| Follow one service | `docker compose logs -f data-api` |
| Check status | `docker compose ps` |
| Stop | `docker compose down` |
| Stop and delete the database | `docker compose down -v` |

## Database

The `db` service runs PostgreSQL 18. On first start, the container runs the scripts in `DbFiles/init/` in alphabetical order:

1. **`00_create_app_role.sh`** creates the `accounts_app` role, using the password from `DbAppPassword.txt`, and grants it read access to every table created afterwards.
2. **`01_schema_and_seed.sql`** creates the schema and loads sample data.

These scripts run **only when the data volume is empty**. To re-run them after changing a script or a password:

```bash
docker compose down -v
docker compose up --build
```

`down -v` deletes all database data.

**Roles.** `accounts_admin` is the superuser and is used only to initialise the database. The Data API connects as `accounts_app`, which can read data but cannot change the schema.

**Schema.**

```mermaid
erDiagram
    customers ||--o{ accounts : owns
    account_statuses ||--o{ accounts : "status of"
    accounts ||--o{ transactions : has
    transaction_types ||--o{ transactions : categorises

    customers {
        int customer_id PK "identity"
        varchar(100) first_name
        varchar(100) last_name
        varchar(255) identity_subject UK "nullable"
    }

    accounts {
        int account_id PK "identity"
        int customer_id FK
        numeric balance "numeric(19,4)"
        numeric overdraft_limit "numeric(19,4)"
        timestamptz date_opened
        char(3) currency_code
        int account_status_id FK
    }

    account_statuses {
        int account_status_id PK
        varchar(50) title UK
    }

    transactions {
        bigint transaction_id PK "identity"
        uuid idempotency_key UK
        int account_id FK
        timestamptz occurred_at
        numeric amount "numeric(19,4)"
        varchar(140) counterparty_name "nullable"
        int transaction_type_id FK
        varchar(140) description
    }

    transaction_types {
        int transaction_type_id PK
        varchar(50) title UK
    }
```

`customers` own `accounts`. Each account has a currency, an overdraft limit and a status. `transactions` form an immutable, single-entry ledger: amounts are signed (positive in, negative out), each transaction has a category from `transaction_types`, and a unique `idempotency_key` prevents the same transaction being recorded twice. Each account's stored `balance` equals the sum of its transactions.

**Queries.** The Data API uses inline SQL in its repositories rather than database functions or stored procedures. Because the Data API is the only path to the database, every query lives in one place next to the code that uses it.

## gRPC contracts

The contracts live in `protos/` and are compiled into both the Data API (server) and the Orchestration API (client).

| Service | RPC | Kind |
|---|---|---|
| `accounts.v1.AccountsService` | `GetAccount` | Unary |
| `accounts.v1.AccountsService` | `GetAccounts` | Unary |
| `statements.v1.StatementsService` | `GetTransactions` | Server streaming |

The Data API listens on two ports: **8080** (HTTP/1.1) for `/health`, and **8081** (HTTP/2) for gRPC. Every gRPC call needs an `x-api-key` header.

### Statement streaming

`GetTransactions` streams an account's transactions one message per transaction, in date order. Rows are read from PostgreSQL, mapped and written to the gRPC response one at a time, so no layer holds the whole statement in memory. If the client disconnects or its deadline expires, cancellation flows back to the database query.

The service checks that the account exists **before** streaming, so a missing account returns `NotFound` rather than an empty stream. An invalid account ID returns `InvalidArgument`.

## Tests

```bash
dotnet test
```

Docker must be running: the integration tests use Testcontainers to start a disposable PostgreSQL 18 container, initialised with the same scripts in `DbFiles/init/` as Docker Compose. The Data API then runs in memory, and the tests call it through a real gRPC client, including the streaming RPC.

## Design notes

- **One responsibility per service.** The frontend never sees the database, and only the Data API holds database credentials.
- **Service-to-service authentication.** Each API validates an `x-api-key` header, with a separate key per hop.
- **Secrets as files, not environment variables.** Passwords and keys are Docker secrets mounted under `/run/secrets`, so they don't appear in `docker inspect` or process environments. The .NET services read them through the key-per-file configuration provider. Only the secret part of the database connection (the password) is a secret; host, database and username are ordinary configuration.
- **Fail fast on configuration.** Every service validates its required settings at startup.
- **Least privilege in the database.** The Data API connects as `accounts_app`, a read-only role, never as the superuser.
- **One error convention.** Expected outcomes (not found, invalid input) are returned as results and translated to gRPC status codes in one place. Unexpected result states are logged once on the server, and the caller receives a generic message with a trace ID.

## Troubleshooting

**`bind source path does not exist: .../secrets/...`**: a secret file is missing or misnamed. Compare `ls secrets` with the table above. Names are case-sensitive.

**`password authentication failed for user "accounts_app"`**: the role doesn't exist or has a different password. PostgreSQL returns the same error in both cases. Usually the database was initialised before `DbAppPassword.txt` had its current value. Reset the volume with `docker compose down -v`.

**The database starts empty, or `accounts_app` doesn't exist**: check `docker compose logs db` for both init scripts. If neither ran, the volume already contained data; reset it. If the folder `/docker-entrypoint-initdb.d` is empty inside the container, the scripts aren't in `DbFiles/init/`.
## Disclaimer

This project is a simple prototype created for demonstration purposes only. It is provided "as is", without warranty of any kind. It is not intended to be used as-is in production. Before any public or commercial deployment, review the security configuration, secrets management, database setup, authentication flow, error handling, logging and infrastructure settings.