using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Interfaces.Repositories;
using AccountsAPI.Application.Interfaces.Services;
using AccountsAPI.Application.Services;
using AccountsAPI.Application.Services.Accounts;
using AccountsAPI.Application.Services.Statements;
using AccountsAPI.Infrastructure.Repositories;
using AccountsAPI.Infrastructure.Security;
using AccountsAPI.Presentation.Authentication;
using AccountsAPI.Presentation.Grpc;
using AccountsAPI.Presentation.Models.Authentication;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.Configure(o =>
    o.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);
builder.Logging.AddSimpleConsole(o => o.IncludeScopes = true);

// Loads Docker/container-mounted secrets from /run/secrets.
// Each file name becomes a configuration key, and each file's contents become the value.
builder.Configuration.AddKeyPerFile(
    directoryPath: "/run/secrets",
    optional: true);

builder.Services.AddOptions<ApiKeyOptions>()
    .Bind(builder.Configuration)
    .Validate(options => !string.IsNullOrWhiteSpace(options.AccountsApiKey), "Some API keys are missing")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.SchemeName,
        options => { options.HeaderName = "x-api-key"; });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireApiKey", policy =>
    {
        policy.AuthenticationSchemes.Add(ApiKeyAuthenticationOptions.SchemeName);
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddGrpc();

var baseConnectionString =
    builder.Configuration.GetConnectionString("AccountsDb")
    ?? throw new InvalidOperationException("Missing connection string: AccountsDb");
var dbPassword =
    builder.Configuration["Database:AppPassword"]
    ?? throw new InvalidOperationException("Missing configuration value: Database:AppPassword");

var accountsDbConnectionString = new NpgsqlConnectionStringBuilder(baseConnectionString)
{
    Password = dbPassword
}.ConnectionString;

builder.Services.AddNpgsqlDataSource(accountsDbConnectionString);

// Reports the API as healthy only when it can connect to the database.
builder.Services.AddHealthChecks().AddNpgSql(accountsDbConnectionString);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddScoped<IStatementRepository, StatementRepository>();
builder.Services.AddScoped<IStatementsService, StatementsService>();

var app = builder.Build();

// Keep the health endpoint unauthenticated so Docker can check container health without an API key.
app.MapHealthChecks("/health");
app.MapGrpcService<AccountsGrpcService>().RequireAuthorization("RequireApiKey");
app.MapGrpcService<StatementGrpcService>().RequireAuthorization("RequireApiKey");
app.Run();