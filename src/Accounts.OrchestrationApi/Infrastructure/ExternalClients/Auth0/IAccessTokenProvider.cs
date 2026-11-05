namespace PortfolioApplicationAPI.Infrastructure.ExternalClients.Auth0;

internal interface IAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken ct);
}