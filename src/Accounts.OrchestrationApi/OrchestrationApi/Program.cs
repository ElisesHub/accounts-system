using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
using OrchestrationApi.Presentation.Authorization;
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
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.SchemeName,
        options => { options.HeaderName = "x-api-key"; });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.RequireApiKey, policy =>
    {
        policy.AuthenticationSchemes.Add(ApiKeyAuthenticationOptions
            .SchemeName);
        policy.RequireAuthenticatedUser();
    })
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(ApiKeyAuthenticationOptions.SchemeName)
        .RequireAuthenticatedUser()
        .Build());

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
    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<ExternalApiOptions>>()
            .Value;
        var handler = new SocketsHttpHandler();

        // If a CA is configured, trust only that CA for the Data API connection.
        // Otherwise the OS trust store is used (e.g. the .NET dev certificate locally).
        if (string.IsNullOrWhiteSpace(options.CaCertificatePath)) return handler;
        var ca = X509Certificate2.CreateFromPem(File.ReadAllText(options.CaCertificatePath));

        handler.SslOptions.CertificateChainPolicy = new X509ChainPolicy
        {
            TrustMode = X509ChainTrustMode.CustomRootTrust,  // ignore the OS store
            CustomTrustStore = { ca },                       // trust only our CA
            RevocationMode = X509RevocationMode.NoCheck      // our private CA publishes no revocation list
        };

        return handler;
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
app.UseAuthentication();
app.UseAuthorization();
// liveness only, no dependencies, anonymous for container probes
app.MapHealthChecks("/health/live", new HealthCheckOptions()
{
    Predicate = _ => false
}).AllowAnonymous();
app.MapControllers().RequireAuthorization(AuthPolicies.RequireApiKey);
app.Run();