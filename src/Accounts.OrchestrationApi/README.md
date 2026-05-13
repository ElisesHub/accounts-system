# Accounts Application API

## Overview

Accounts Application API is the application-facing API for the accounts system. It sits between the Portfolio Frontend and the Accounts Database API.

The service receives HTTP requests from the frontend, validates access using an API key, and forwards account-related requests to the Accounts Database API through HTTP.

The application does not connect directly to the database. Database access is handled by the Accounts Database API.

## Architecture

```text
Portfolio Frontend
        ↓ HTTP + API Key
Accounts Application API
        ↓ HTTP + API Key
Accounts Database API
        ↓
MySQL Database
````

## Responsibilities

This project is responsible for:

* Exposing account-related HTTP endpoints to the frontend
* Authenticating requests using API key authentication
* Calling the Accounts Database API through a typed HTTP client
* Returning account data to the frontend
* Handling validation errors in a consistent response format
* Handling unexpected exceptions through a global exception handler
* Providing a health check endpoint
* Keeping the frontend separated from database-facing concerns

## Project Structure

```text
AccountsApplicationAPI
├── Application
├── Domain
├── Infrastructure
└── Presentation
```

Typical responsibilities:

* `Domain` — core account-related domain models and business rules
* `Application` — use-case orchestration, service interfaces, and application services
* `Infrastructure` — HTTP clients for downstream APIs, API key validation, configuration, and external integrations
* `Presentation` — API controllers, authentication setup, authorization policies, error responses, exception handling, and request/response models

## Technologies

* .NET / ASP.NET Core Web API
* Clean Architecture
* Domain-Driven Design
* API key authentication
* Typed `HttpClient`
* Swagger / OpenAPI
* Health checks
* Docker / Docker Compose

## Authentication

The Accounts Application API uses API key authentication.

Requests must include the API key in the following HTTP header:

```text
x-api-key
```

All mapped controllers require the `RequireApiKey` authorization policy.

```csharp
app.MapControllers().RequireAuthorization("RequireApiKey");
```

If the API key is missing or invalid, the request is rejected before reaching the controller action.

## Configuration

The Accounts Application API uses configuration from `appsettings.json`, `appsettings.Development.json`, environment variables, .NET user secrets, and optional container-mounted secret files.

The Accounts Database API base URL is not treated as a secret. For local development, it is configured in `appsettings.Development.json`.

Example:

```json
{
  "AccountsDbApi": {
    "BaseUrl": "http://localhost:5253"
  }
}
```

When running through Docker Compose, the same value is provided by the deployment repository as an environment variable.

In Docker Compose, nested .NET configuration keys are usually represented with double underscores:

```yaml
environment:
  AccountsDbApi__BaseUrl: "http://accountsapi:8080"
```

The value must be a valid absolute URI.

## Secrets

This repository does not contain runtime secrets.

For local development, secrets should be managed with .NET user secrets. These values are stored outside the repository and are not committed to Git.

When the application is run as part of the full accounts system, secret values are provided by a separate deployment repository through Docker Compose.

The application also supports container-mounted secrets through `/run/secrets` when they are provided by the runtime environment. This is optional and mainly intended for containerized deployments.

## Required Local User Secrets

The following .NET user secrets are required for local development:

```text
AccountsApplicationApiKey=
AccountsApiKey=
```

Initialize user secrets from the Accounts Application API project directory:

```bash
dotnet user-secrets init
```

Set the required values:

```bash
dotnet user-secrets set "AccountsApplicationApiKey" "your-accounts-application-api-key"
dotnet user-secrets set "AccountsApiKey" "your-accounts-api-key"
```

Do not commit real API keys or environment-specific credentials to source control.

## API Key Configuration

This service uses two API key values:

```text
AccountsApplicationApiKey
AccountsApiKey
```

`AccountsApplicationApiKey` is used to validate incoming requests from the frontend.

`AccountsApiKey` is used when making outgoing requests to the Accounts Database API.

Both values are required. If either key is missing, the application fails during startup.

## Downstream API Configuration

The Accounts Application API communicates with the Accounts Database API through a typed HTTP client.

The downstream API base URL is configured with:

```text
AccountsDbApi:BaseUrl
```

For local development, this value is defined in `appsettings.Development.json`:

```json
{
  "AccountsDbApi": {
    "BaseUrl": "http://localhost:5253"
  }
}
```

When running through the separate deployment repository, this value is injected by Docker Compose as an environment variable:

```text
AccountsDbApi__BaseUrl
```

Example Docker Compose value:

```text
http://accountsapi:8080
```

`AccountsDbApi:BaseUrl` must be a valid absolute URI.

## API Endpoints

### Health Check

```http
GET /health
```

Returns the health status of the service.

### Accounts

The service exposes account-related endpoints through its controllers.

Expected account endpoint:

```http
GET /api/accounts
```

This endpoint retrieves account data by calling the Accounts Database API.

## Request Flow

A typical request follows this flow:

```text
Portfolio Frontend
  ↓
x-api-key validation
  ↓
Accounts Application API Controller
  ↓
IAccountsService
  ↓
IExternalAccountsClient
  ↓
Accounts Database API
```

The application registers the account service:

```csharp
builder.Services.AddScoped<IAccountsService, AccountsService>();
```

It also registers a typed HTTP client for communication with the Accounts Database API:

```csharp
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>();
```

This keeps the application layer decoupled from the details of the downstream HTTP integration.

## Error Handling

The application uses a global exception handler:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseExceptionHandler();
```

Unexpected errors are handled centrally and returned using a consistent API error response format.

## Validation Errors

Invalid model state responses are customized.

Validation errors return a structured response containing:

```text
Code
Message
FieldErrors
TraceId
```

Example validation error shape:

```json
{
  "code": "ValidationError",
  "message": "One or more validation errors occurred.",
  "fieldErrors": {
    "fieldName": [
      "Validation error message"
    ]
  },
  "traceId": "request-trace-id"
}
```

## Swagger

Swagger is enabled only in development environments.

When running in development, Swagger UI is available through the configured Swagger endpoint.

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Swagger is not enabled in non-development environments.

## Running Locally

Restore dependencies:

```bash
dotnet restore
```

Run the API project:

```bash
dotnet run
```

If running from the solution root, provide the project path:

```bash
dotnet run --project path/to/AccountsApplicationAPI
```

The Accounts Database API must also be running and reachable through the configured `AccountsDbApi:BaseUrl`.

## Running with Docker Compose

This project is designed to be run as part of the full accounts system through a separate deployment repository.

The deployment repository contains the `docker-compose.yaml` file used to start the Portfolio Frontend, the Accounts Application API, the Accounts Database API, and the MySQL database together.

From the deployment repository, run:

```bash
docker compose up
```

The deployment repository is responsible for providing runtime configuration such as service URLs, API keys, environment variables, and Docker secrets.

For Docker Compose, the Accounts Database API base URL is provided as an environment variable:

```text
AccountsDbApi__BaseUrl
```

This repository contains the Accounts Application API source code only. Runtime orchestration, service wiring, environment variables, and secrets are managed outside this repository.

## Configuration Validation

The application validates required configuration at startup.

Startup fails if:

* `AccountsApplicationApiKey` is missing
* `AccountsApiKey` is missing
* `AccountsDbApi:BaseUrl` is missing
* `AccountsDbApi:BaseUrl` is not a valid absolute URI

This helps detect configuration problems before the application starts serving requests.

## Troubleshooting

### The API fails on startup

Check that all required configuration values are present:

```text
AccountsApplicationApiKey
AccountsApiKey
AccountsDbApi:BaseUrl
```

Also check that `AccountsDbApi:BaseUrl` is a valid absolute URI.

### Requests return unauthorized

Check that the request includes the required API key header:

```text
x-api-key
```

Also check that the provided key matches the configured `AccountsApplicationApiKey`.

### The API cannot retrieve accounts

Check that:

* The Accounts Database API is running
* `AccountsDbApi:BaseUrl` points to the correct Accounts Database API service
* The configured `AccountsApiKey` is valid
* The Accounts Application API can reach the Accounts Database API over HTTP
* The Accounts Database API can connect to the MySQL database

### Swagger is not available

Swagger is only enabled in development environments.

Check that the application is running with the development environment setting:

```text
ASPNETCORE_ENVIRONMENT=Development
```

## Security Notes

Secrets are not stored in this repository.

Do not commit:

* API keys
* Environment-specific credentials
* Production configuration values
* Local user-secrets files
* Generated secret files

For local development, use .NET user secrets.

For containerized execution, required secret values are injected by the deployment setup through Docker Compose.

The Accounts Database API base URL is configuration, not a secret.

## Disclaimer

This project is a simple prototype created for demonstration purposes only. It is provided "as is", without warranty of any kind.

The author is not responsible for any issues that may result from the use, modification, deployment, or distribution of this project, including data loss, security issues, or service interruptions.

This project is not intended to be used as-is in a production environment. Before any public or commercial deployment, review the security configuration, secrets management, authentication flow, downstream API configuration, error handling, logs, and infrastructure settings.