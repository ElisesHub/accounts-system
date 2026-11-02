using AccountsAPI.Application.Dtos.Accounts;
using AccountsAPI.Application.Models;

namespace AccountsAPI.Application.Interfaces.Services;

public interface IAccountsService
{
    Task<Result<AccountDto>> GetAccountAsync(int accountId, CancellationToken ct);
    Task<Result<IReadOnlyList<AccountDto>>> GetAccountsAsync(CancellationToken ct);
}