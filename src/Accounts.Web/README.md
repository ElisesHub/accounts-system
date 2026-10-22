
# Account System Frontend

## Overview

Portfolio Frontend is the user-facing web application for the accounts system. It renders the UI, handles Auth0 authentication, and communicates with the Accounts Application API through HTTP requests.

The project follows Clean Architecture and Domain-Driven Design principles by separating presentation, application, domain, and infrastructure concerns.

The frontend does not connect directly to the database. Account data is retrieved through the Accounts Application API.

## Architecture

```text
Portfolio Frontend
        ↓ HTTP
Accounts Application API
        ↓ HTTP
Accounts Database API
        ↓
MySQL Database
````

## Responsibilities

This project is responsible for:

* Rendering the frontend user interface
* Handling Auth0-based authentication
* Calling the Accounts Application API through a typed HTTP client
* Displaying account data returned by the backend
* Handling user-facing errors and status-code pages
* Keeping presentation, application, domain, and infrastructure concerns separated

## Project Structure

```text
PortfolioFe
├── Application
├── Domain
├── Infrastructure
└── Presentation
```

Typical responsibilities:

* `Presentation` — Razor Pages, MVC controllers, views, routing, and UI concerns
* `Application` — application services, interfaces, and use-case logic
* `Domain` — domain models and business rules
* `Infrastructure` — external HTTP clients, configuration, and security integrations


## Technologies

* .NET / ASP.NET Core
* Razor Pages
* MVC Controllers and Views
* Clean Architecture
* Domain-Driven Design
* Auth0 authentication
* Typed `HttpClient`
* Docker / Docker Compose

## Authentication

The frontend uses Auth0 for web application authentication.

The following configuration values are required:

```text
Auth0:Domain
Auth0:ClientId
Auth0:ClientSecret
```

If any of these values are missing, the application fails during startup.

## Configuration and Secrets

This repository does not contain runtime secrets or environment-specific configuration values.

For local development, secrets should be managed with .NET user secrets. These values are stored outside the repository and are not committed to Git.

When the application is run as part of the full accounts system, runtime configuration is provided by a separate deployment repository through Docker Compose.

The frontend also supports container-mounted secrets through `/run/secrets` when they are provided by the runtime environment. This is optional and mainly intended for containerized deployments.

## Required Local User Secrets

The following .NET user secrets are required for local development:

```text
Auth0:Domain=
Auth0:ClientSecret=
Auth0:ClientId=
AccountsApplicationApiKey=
```

Initialize user secrets from the frontend project directory:

```bash
dotnet user-secrets init
```

Set the required values:

```bash
dotnet user-secrets set "Auth0:Domain" "your-auth0-domain"
dotnet user-secrets set "Auth0:ClientSecret" "your-auth0-client-secret"
dotnet user-secrets set "Auth0:ClientId" "your-auth0-client-id"
dotnet user-secrets set "AccountsApplicationApiKey" "your-accounts-application-api-key"
```


## External API Configuration

The frontend communicates with the Accounts Application API through a typed HTTP client.

The Accounts Application API base URL is configured through:

```text
ExternalApi:BaseUrl
```

When running through the separate deployment repository, this value is injected by Docker Compose and usually points to the Accounts Application API service name inside the Docker network.

Example:

```text
http://accountsapplicationapi:8080
```

`ExternalApi:BaseUrl` must be a valid absolute URI.

## API Key Configuration

The frontend requires an API key when communicating with the Accounts Application API.

Required configuration value:

```text
AccountsApplicationApiKey
```

The value should be provided through .NET user secrets during local development or injected by the deployment setup when running with Docker Compose.

## Running Locally

Restore dependencies:

```bash
dotnet restore
```

Run the frontend project:

```bash
dotnet run
```

If running from the solution root, provide the project path:

```bash
dotnet run --project path/to/PortfolioFe
```

The Accounts Application API must also be running and reachable through the configured `ExternalApi:BaseUrl`.

## Running with Docker Compose

This project is designed to be run as part of the full accounts system through a separate deployment repository.

The deployment repository contains the `docker-compose.yaml` file used to start the frontend, the Accounts Application API, the Accounts Database API, and the MySQL database together.

From the deployment repository, run:

```bash
docker compose up
```

The deployment repository is responsible for providing runtime configuration such as service URLs, API keys, Auth0 settings, environment variables, and Docker secrets.

This frontend repository contains the application source code only. Runtime orchestration, service wiring, environment variables, and secrets are managed outside this repository.

## Request Flow

A typical account request follows this flow:

```text
User
  ↓
Portfolio Frontend
  ↓
IAccountsService
  ↓
IExternalAccountsClient
  ↓
Accounts Application API
```

The frontend registers the account application service:

```csharp
builder.Services.AddScoped<IAccountsService, AccountsService>();
```

It also registers a typed HTTP client for communication with the Accounts Application API:

```csharp
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>();
```

This keeps the frontend decoupled from the implementation details of the downstream HTTP integration.

## Error Handling

In non-development environments, the application uses a centralized exception handler:

```text
/Error
```

Status-code responses such as 404, 403, and 500 are handled through:

```text
/Error/StatusCode?code={statusCode}
```

The request pipeline includes:

```text
Static Files
Routing
Authentication
Authorization
Razor Pages
MVC Controller Routes
```

## Configuration Validation

The application validates required configuration at startup.

Startup fails if:

* `Auth0:Domain` is missing
* `Auth0:ClientId` is missing
* `Auth0:ClientSecret` is missing
* `AccountsApplicationApiKey` is missing
* `ExternalApi:BaseUrl` is missing
* `ExternalApi:BaseUrl` is not a valid absolute URI

This helps detect configuration problems before the application starts serving requests.

## Troubleshooting

### The frontend cannot retrieve accounts

Check that:

* The Accounts Application API is running
* `ExternalApi:BaseUrl` points to the correct service
* The required API key is configured
* The frontend can reach the Accounts Application API over HTTP
* The Accounts Application API can reach its own downstream services

### The application fails on startup

Check that all required configuration values are present:

```text
Auth0:Domain
Auth0:ClientId
Auth0:ClientSecret
AccountsApplicationApiKey
ExternalApi:BaseUrl
```

Also check that `ExternalApi:BaseUrl` is a valid absolute URI.

### Auth0 login does not work

Check that:

* The Auth0 domain is correct
* The client ID and client secret are correct
* The callback URL is configured correctly in Auth0
* The logout URL is configured correctly in Auth0
* The application is running on the expected host and port

## Security Notes

Secrets are not stored in this repository.

Do not commit:

* Auth0 client secrets
* API keys
* Environment-specific credentials
* Production configuration values
* Local user-secrets files
* Generated secret files

For local development, use .NET user secrets.

For containerized execution, required values are injected by the deployment setup through Docker Compose.

## Disclaimer

This project is a simple prototype created for demonstration purposes only. It is provided "as is", without warranty of any kind.

The author is not responsible for any issues that may result from the use, modification, deployment, or distribution of this project, including data loss, security issues, or service interruptions.

This project is not intended to be used as-is in a production environment. Before any public or commercial deployment, review the security configuration, secrets management, database configuration, authentication flow, error handling, logs, and infrastructure settings.