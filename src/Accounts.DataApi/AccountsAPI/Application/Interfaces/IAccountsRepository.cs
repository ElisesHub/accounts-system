using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces;

public interface IAccountsRepository
{
    Task<Account?> GetAccountAsync(AccountId accountId, CancellationToken ct);
    Task<IReadOnlyList<Account>> GetAccountsAsync(CancellationToken ct);
}