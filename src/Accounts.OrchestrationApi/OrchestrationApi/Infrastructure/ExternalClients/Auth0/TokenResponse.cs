using System.Text.Json.Serialization;

namespace OrchestrationApi.Infrastructure.ExternalClients.Auth0;

internal sealed record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);   // seconds