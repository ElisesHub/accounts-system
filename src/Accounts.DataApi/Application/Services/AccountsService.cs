using AccountsAPI.Application.Interfaces;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Infrastructure.Repositories;

namespace AccountsAPI.Application.Services;

public class AccountsService(IAccountsRepository accountsRepository) : IAccountsService
{
    public async Task<Account?> GetAccountAsync(string accountId)
    {
        var account = await accountsRepository.GetAccountAsync(accountId);
        if(account == null) throw new Exception("Account not found");
        return account;
    }

    public async Task<List<Account>?> GetAccountsAsync()
    {
        var accounts = await accountsRepository.GetAccountsAsync();
        if(accounts == null) throw new Exception("No accounts found");
        return accounts;
    }
}