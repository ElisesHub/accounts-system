using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Services;
using AccountsAPI.Infrastructure.Repositories;
using AccountsAPI.Infrastructure.Security;
using AccountsAPI.Presentation.Authentication;
using AccountsAPI.Presentation.ExceptionHandling;
using AccountsAPI.Presentation.Models;
using AccountsAPI.Presentation.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Ensures automatic model validation failures use the API's standard error response shape.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new ApiErrorResponse
            {
                Code = ApiErrorCodes.ValidationError.ToString(),
                Message = "One or more validation errors occurred.",
                FieldErrors = errors,
                TraceId = context.HttpContext.TraceIdentifier
            };

            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var accountsDbConnectionString =
    builder.Configuration.GetConnectionString("AccountsDb")
    ?? throw new InvalidOperationException("Missing connection string: AccountsDb");

builder.Services.AddMySqlDataSource(accountsDbConnectionString);

// Reports the API as healthy only when it can connect to the Accounts database.
builder.Services.AddHealthChecks()
    .AddMySql(accountsDbConnectionString);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Keep the health endpoint unauthenticated so Docker can check container health without an API key.
app.MapHealthChecks("/health");

app.MapControllers().RequireAuthorization("RequireApiKey");

// app.UseHttpsRedirection();

app.Run();