using AccountsAPI.Application.Interfaces.Repositories;
using AccountsAPI.Application.Interfaces.Services;
using AccountsAPI.Application.Services.Accounts;
using AccountsAPI.Application.Services.Statements;
using AccountsAPI.Infrastructure.Repositories;
using AccountsAPI.Presentation.Authorization;
using AccountsAPI.Presentation.Grpc;
using AccountsAPI.Presentation.Interceptors;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.Configure(o =>
    o.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);

if (builder.Environment.IsDevelopment())
    builder.Logging.AddSimpleConsole(o => o.IncludeScopes = true);
else
    builder.Logging.AddJsonConsole(o =>
    {
        o.IncludeScopes = true;
        o.UseUtcTimestamp =  true;
        o.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    });

// Loads Docker/container-mounted secrets from /run/secrets.
// Each file name becomes a configuration key, and each file's contents become the value.
builder.Configuration.AddKeyPerFile(
    directoryPath: "/run/secrets",
    optional: true);

builder.Services.AddOptions<Auth0Options>()
    .BindConfiguration(Auth0Options.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<Auth0Options>>((
            options, auth0)
        =>
    {
        options.Authority = $"https://{auth0.Value.Domain}/";
        options.Audience = auth0.Value.Audience;
        options.MapInboundClaims = false; // opt out of backward compatibility defaults
    });

builder.Services.AddAuthorizationBuilder()
    // Set up policies
    .AddPolicy(AuthPolicies.ReadAccounts, policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(authorizationHandlerContext =>
            authorizationHandlerContext.User.FindFirst("scope")?.Value.Split(' ').Contains("read:accounts") == true))
    // Make sure all endpoints require authenticated user by default
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// Translate unhandled exceptions into gRPC Internal errors with a trace ID.
builder.Services.AddGrpc(options => { options.Interceptors.Add<ExceptionInterceptor>(); });

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

// this check backs health/ready
builder.Services.AddHealthChecks().AddNpgSql(name: "accounts-db", tags: ["ready"]);

builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IStatementRepository, StatementRepository>();
builder.Services.AddScoped<IStatementsService, StatementsService>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
// liveness, responds, dependencies not taken into account. Restart if this fails. Anonymous access allowed.
app.MapHealthChecks("/health/live", new HealthCheckOptions()
{
    Predicate = _ => false
}).AllowAnonymous();

// readiness, checks database, don't route traffic if this fails. Anonymous access allowed.
app.MapHealthChecks("/health/ready", new HealthCheckOptions()
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

app.MapGrpcService<AccountsGrpcService>().RequireAuthorization(AuthPolicies.ReadAccounts);
app.MapGrpcService<StatementGrpcService>().RequireAuthorization(AuthPolicies.ReadAccounts);

app.Run();