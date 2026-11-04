using Microsoft.Extensions.Options;

namespace PortfolioApplicationAPI.Infrastructure.ExternalClients.Auth0;

internal sealed class Auth0TokenProvider(IHttpClientFactory httpClientFactory, IOptions<Auth0Options> options)
    : IAccessTokenProvider
{
    private string? _token;

    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromSeconds(60);

    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (_token is not null && DateTimeOffset.UtcNow < _expiresAt) return _token;

        await _semaphoreSlim.WaitAsync(ct);
        try
        {
            if (_token is not null && DateTimeOffset.UtcNow < _expiresAt)
                return _token;

            var client = httpClientFactory.CreateClient("Auth0");
            var request = new TokenRequest("client_credentials", options.Value.ClientId, options.Value.ClientSecret,
                options.Value.Audience);

            using var response = await client.PostAsJsonAsync("oauth/token", request, ct);

            response.EnsureSuccessStatusCode();
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct) ??
                                throw new InvalidOperationException("No token returned from the client.");

            _token = tokenResponse.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn) - RefreshMargin;
            return _token;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}