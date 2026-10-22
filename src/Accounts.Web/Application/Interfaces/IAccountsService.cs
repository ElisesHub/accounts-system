using PortfolioFe.Domain.Bank;

namespace PortfolioFe.Application.Interfaces;

public interface IAccountsService
{
    Task<Account?> GetAccountAsync(string accountId);
    Task<IEnumerable<Account>?> GetAccountsAsync();
}