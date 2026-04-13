using PortfolioFe.Domain.Bank;

namespace PortfolioFe.Application.Services;

public interface IAccountsService
{
    Task<Account?> GetAccountAsync(string accountId);
    Task<List<Account>?> GetAccountsAsync();
}