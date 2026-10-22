using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Presentation.Models;
using PortfolioApplicationAPI.Presentation.Models.Authentication;

namespace PortfolioApplicationAPI.Presentation.Authentication;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyValidator apiKeyValidator, IHostEnvironment environment)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key header is missing."));
        }

        var providedApiKey = headerValues.ToString();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key is empty."));
        }

        if (!apiKeyValidator.IsValid(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ApiKeyClient")
        };

        var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationOptions.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            Code = "UNAUTHORIZED",
            Message = "Resource requires authentication.",
            TraceId = Context.TraceIdentifier,
            DeveloperMessage = environment.IsDevelopment() ?  "Ensure that the API key is set in the request headers." : null
        };

        return Response.WriteAsJsonAsync(response);
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            Code = "FORBIDDEN",
            Message = "Access to resource denied.",
            TraceId = Context.TraceIdentifier
        };

        return Response.WriteAsJsonAsync(response);
    }
}