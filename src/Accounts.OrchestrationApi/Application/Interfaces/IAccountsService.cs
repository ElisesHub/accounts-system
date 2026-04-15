
using AccountsApplicationAPI.Models;

namespace AccountsAPI.Services;

public interface IAccountsService
{
    Task<Account?> GetAccountAsync(string accountId, string incomingApiKey);
    Task<IEnumerable<Account>?> GetAccountsAsync(string incomingApiKey);
}