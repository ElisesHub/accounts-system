using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchestrationApi.Application.Interfaces;
using OrchestrationApi.Application.Services;
using OrchestrationApi.Infrastructure.ExternalClients.Auth0;
using OrchestrationApi.Infrastructure.ExternalClients.Configuration;
using OrchestrationApi.Infrastructure.ExternalClients.gRPC;
using OrchestrationApi.Infrastructure.Security;
using OrchestrationApi.Infrastructure.Security.ApiKeys;
using OrchestrationApi.Presentation.Authentication;
using OrchestrationApi.Presentation.ExceptionHandling;
using OrchestrationApi.Presentation.Models;
using OrchestrationApi.Presentation.Models.Authentication;
using AccountsGrpc = Accounts.Api.V1.AccountsService;

var builder = WebApplication.CreateBuilder(args);

// Loads Docker/container-mounted secrets from /run/secrets.
// Each file name becomes a configuration key; each file's contents become the value.
builder.Configuration.AddKeyPerFile(
    directoryPath: "/run/secrets",
    optional: true);
builder.Services.AddOptions<ApiKeyOptions>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<Auth0Options>()
    .Bind(builder.Configuration.GetSection(Auth0Options.SectionName))
    .ValidateDataAnnotations()
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
                TraceId = Activity.Current?.TraceId.ToString() ?? "Not Found"
            };

            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddHttpClient("Auth0", (serviceProvider, client) =>
{
    var auth0 = serviceProvider.GetRequiredService<IOptions<Auth0Options>>().Value;
    client.BaseAddress = new Uri($"https://{auth0.Domain}/");
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAccessTokenProvider, Auth0TokenProvider>();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddGrpcClient<AccountsGrpc.AccountsServiceClient>((serviceProvider, grpcOptions) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<ExternalApiOptions>>()
            .Value;
        grpcOptions.Address = new Uri(options.BaseUrl);
    })
    .AddCallCredentials(async (context, metadata, serviceProvider) =>
    {
        var tokenProvider = serviceProvider.GetRequiredService<IAccessTokenProvider>();
        var token = await tokenProvider.GetAccessTokenAsync(context.CancellationToken);
        metadata.Add("Authorization", $"Bearer {token}");
    })
    .ConfigureChannel(channel =>
    {
        // TODO: implement TLS between services
        channel.UnsafeUseInsecureChannelCallCredentials = true;
    });
builder.Services.AddScoped<IExternalAccountsClient, AccountsClient>();
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


app.Run();