using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces.Repositories;

public interface IAccountsRepository
{
    Task<Account?> GetAccountAsync(AccountId accountId, CancellationToken ct);
    Task<IReadOnlyList<Account>> GetAccountsAsync(CancellationToken ct);

    Task<bool> AccountExistsAsync(AccountId accountId, CancellationToken ct);
}