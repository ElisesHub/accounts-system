using AccountsAPI.Services;
using AccountsApplicationAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Infrastructure.ExternalClients;
using PortfolioApplicationAPI.Infrastructure.ExternalClients.Configuration;
using PortfolioApplicationAPI.Infrastructure.Security;
using PortfolioApplicationAPI.Infrastructure.Security.ApiKeys;
using PortfolioApplicationAPI.Presentation.Authentication;
using PortfolioApplicationAPI.Presentation.ExceptionHandling;
using PortfolioApplicationAPI.Presentation.Models;
using PortfolioApplicationAPI.Presentation.Models.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Loads Docker/container-mounted secrets from /run/secrets.
// Each file name becomes a configuration key; each file's contents become the value.
builder.Configuration.AddKeyPerFile(
    directoryPath: "/run/secrets",
    optional: true);
builder.Services.AddOptions<ApiKeyOptions>()
    .Bind(builder.Configuration)
    .Validate(options => !string.IsNullOrWhiteSpace(options.AccountsApiKey) && !string.IsNullOrWhiteSpace(options.AccountsApplicationApiKey), "Some API keys are missing")
    .ValidateOnStart();
builder.Services.AddOptions<ExternalApiOptions>()
    .Bind(builder.Configuration.GetSection(ExternalApiOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "External API base URL is missing")
    .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "External API base URL is not valid")
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
        policy.AuthenticationSchemes.Add(ApiKeyAuthenticationOptions
            .SchemeName);
        policy.RequireAuthenticatedUser();
    });
});
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage)
                        .ToArray()
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<IOptions<ExternalApiOptions>>()
        .Value;

    if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
    {
        throw new InvalidOperationException(
            $"ExternalApi:BaseUrl must be a valid absolute URI. Current value: '{options.BaseUrl}'.");
    }

    client.BaseAddress = baseUri;
});
builder.Services.AddHealthChecks();
var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapControllers().RequireAuthorization("RequireApiKey");

// app.UseHttpsRedirection();


app.Run();