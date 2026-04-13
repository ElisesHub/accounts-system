
using PortfolioApplicationAPI.Models;

namespace AccountsAPI.Services;

public interface IAccountsService
{
    Task<Account?> GetAccountAsync(string accountId);
    Task<IEnumerable<Account>?> GetAccountsAsync();
}