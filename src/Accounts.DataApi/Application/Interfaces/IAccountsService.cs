using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces;

public interface IAccountsService
{
    Task<Account?> GetAccountAsync(string accountId, string incomingApiKey);
    Task<List<Account>?> GetAccountsAsync(string incomingApiKey);
}