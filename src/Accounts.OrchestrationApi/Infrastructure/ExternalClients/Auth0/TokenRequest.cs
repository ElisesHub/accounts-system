using System.Text.Json.Serialization;

namespace PortfolioApplicationAPI.Infrastructure.ExternalClients.Auth0;

internal sealed record TokenRequest(
    [property: JsonPropertyName("grant_type")] string GrantType,
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("client_secret")] string ClientSecret,
    [property: JsonPropertyName("audience")] string Audience);