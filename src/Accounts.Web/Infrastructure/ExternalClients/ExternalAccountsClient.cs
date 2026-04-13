using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Models;

namespace PortfolioApplicationAPI.Infrastructure.ExternalClients;

public class ExternalAccountsClient(HttpClient httpClient) : IExternalAccountsClient
{
    private const string AccountsUrl = "api/accounts";

    public async Task<Account?> GetAccountAsync(string id)
    {
        var response = await httpClient.GetAsync($"{httpClient.BaseAddress}/{AccountsUrl}/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Account>();
    }

    public async Task<IEnumerable<Account>?> GetAccountsAsync()
    {
        var response = await httpClient.GetAsync($"{AccountsUrl}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Account>>();
    }


}