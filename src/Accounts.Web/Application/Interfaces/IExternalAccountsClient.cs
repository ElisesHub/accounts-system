
using PortfolioFe.Domain.Bank;

namespace PortfolioFe.Application.Interfaces;

public interface IExternalAccountsClient
{
    Task<Account?> GetAccountAsync(string id);
    Task<IEnumerable<Account>?> GetAccountsAsync();
}