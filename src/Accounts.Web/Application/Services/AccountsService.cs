using PortfolioFe.Application.Interfaces;
using PortfolioFe.Domain.Bank;

namespace PortfolioFe.Application.Services;

public class AccountsService(IExternalAccountsClient externalAccountsClient) : IAccountsService
{
    public async Task<Account?> GetAccountAsync(string accountId)
    {
        var account = await externalAccountsClient.GetAccountAsync(accountId);

        return account;
    }

    public async Task<IEnumerable<Account>?> GetAccountsAsync()
    {
        var accounts = await externalAccountsClient.GetAccountsAsync();

        return accounts;
    }
}