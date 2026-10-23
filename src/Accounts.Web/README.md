# Accounts Web

The user-facing web application of the [accounts system](../../README.md). It renders the UI, handles sign-in with Auth0, and retrieves account data from the [Orchestration API](../Accounts.OrchestrationApi/README.md). It has no direct access to the other services' data or the database.

```text
Browser  →  Accounts Web  →  Orchestration API  →  Accounts Data API  →  MySQL
```

## Responsibilities

- Render the user interface with Razor Pages and MVC views
- Authenticate users with Auth0
- Call the Orchestration API through a typed `HttpClient`, sending an API key
- Display account data
- Show user-facing error and status-code pages

## Project structure

```text
src/Accounts.Web/
├── Domain/           Domain models and business rules
├── Application/      Application services and interfaces
├── Infrastructure/   HTTP client for the Orchestration API, configuration, security integrations
├── Presentation/     Razor Pages, MVC controllers and views, routing
└── wwwroot/          Static assets
```

Requests flow Page or controller → `IAccountsService` → `IExternalAccountsClient` → Orchestration API:

```csharp
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>();
```

## Technologies

ASP.NET Core (.NET 10), Razor Pages, MVC, Clean Architecture, Domain-Driven Design, Auth0, typed `HttpClient`.

## Configuration

| Key | Purpose | Secret |
|---|---|---|
| `Auth0:Domain` | Auth0 tenant domain | No |
| `Auth0:ClientId` | Auth0 application client ID | Yes |
| `Auth0:ClientSecret` | Auth0 application client secret | Yes |
| `AccountsApplicationApiKey` | Key sent to the Orchestration API | Yes |
| `ExternalAccountsApi:BaseUrl` | Base URL of the Orchestration API, as an absolute URI | No |

Startup fails if any value is missing or the base URL is not a valid absolute URI.

**Local development**: non-secret values are in `appsettings.Development.json`:

```json
{
  "ExternalAccountsApi": {
    "BaseUrl": "http://localhost:5182"
  },
  "Auth0": {
    "Domain": "your-tenant.eu.auth0.com"
  }
}
```

The secrets are .NET user secrets:

```bash
cd src/Accounts.Web
dotnet user-secrets set "Auth0:ClientId" "your-auth0-client-id"
dotnet user-secrets set "Auth0:ClientSecret" "your-auth0-client-secret"
dotnet user-secrets set "AccountsApplicationApiKey" "your-accounts-application-api-key"
```

**In Docker**, the client ID, client secret and API key are Docker secrets mounted under `/run/secrets`. The domain and base URL are set in `docker-compose.yaml`:

```yaml
Auth0__Domain: your-tenant.eu.auth0.com
ExternalAccountsApi__BaseUrl: "http://orchestration-api:8080"
```

## Auth0 setup

In the Auth0 application settings, register the URLs the app runs on:

| Setting | Docker Compose | `dotnet run` |
|---|---|---|
| Allowed Callback URLs | `http://localhost:1000/callback` | `http://localhost:5236/callback` |
| Allowed Logout URLs | `http://localhost:1000` | `http://localhost:5236` |

## Error handling

Outside Development, unhandled exceptions are shown on `/Error`, and status codes such as 403, 404 and 500 on `/Error/StatusCode?code={statusCode}`. The `httpProd` launch profile runs the app locally as Production, to check these pages without Docker.

## Running locally

With the Orchestration API running on `http://localhost:5182`:

```bash
dotnet run --project src/Accounts.Web
```

The app is available at `http://localhost:5236`. To run the whole system, use Docker Compose from the repository root, and the app is then at `http://localhost:1000`. See the [root README](../../README.md).

## Troubleshooting

**Startup fails**: check all five configuration values are set and the base URL is an absolute URI.

**Accounts cannot be retrieved**: check the Orchestration API is running, that `ExternalAccountsApi:BaseUrl` points to it, and that `AccountsApplicationApiKey` matches the key it expects. In Docker, check `docker compose logs orchestration-api` and `docker compose logs data-api` to see where the request failed.

**Auth0 login fails**: check the domain, client ID and secret, and that the callback and logout URLs registered in Auth0 match the host and port the app runs on.