using PortfolioFe.Models;

namespace PortfolioFe.Services;

public class AccountsService(ApiService apiService) : IAccountsService
{
    public async Task<Account?> GetAccountAsync(string accountId)
    {
        var account = await apiService.GetAccountAsync(accountId);
        if(account == null) throw new Exception("Account not found");
        return account;
    }

    public async Task<List<Account>?> GetAccountsAsync()
    {
        var accounts = await apiService.GetAccountsAsync();
        if(accounts == null) throw new Exception("No accounts found");
        return accounts;
    }
}