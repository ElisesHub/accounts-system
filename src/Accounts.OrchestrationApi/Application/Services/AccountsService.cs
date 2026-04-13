
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Models;

namespace AccountsAPI.Services;

public class AccountsService(IExternalAccountsClient externalAccountsClient ) : IAccountsService
{
    public async Task<Account?> GetAccountAsync(string accountId)
    {
        var account = await externalAccountsClient.GetAccountAsync(accountId);
        if(account == null) throw new Exception("Account not found");
        return account;
    }

    public async Task<IEnumerable<Account>?> GetAccountsAsync()
    {
        var accounts = await externalAccountsClient.GetAccountsAsync();
        if(accounts == null) throw new Exception("No accounts found");
        return accounts;
    }
}