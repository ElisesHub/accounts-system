using AccountsApplicationAPI.Models;

namespace AccountsApplicationAPI.Application.Interfaces;

public interface IExternalAccountsClient
{
    Task<Account?> GetAccountAsync(string id);
    Task<IEnumerable<Account>?> GetAccountsAsync();
}