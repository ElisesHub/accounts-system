using PortfolioApplicationAPI.Models;

namespace PortfolioApplicationAPI.Application.Interfaces;

public interface IExternalAccountsClient
{
    Task<Account?> GetAccountAsync(string id);
    Task<IEnumerable<Account>?> GetAccountsAsync();
}