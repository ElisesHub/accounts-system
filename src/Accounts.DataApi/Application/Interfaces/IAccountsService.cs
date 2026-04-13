using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces;

public interface IAccountsService
{
    Task<Account?> GetAccountAsync(string accountId);
    Task<List<Account>?> GetAccountsAsync();
}