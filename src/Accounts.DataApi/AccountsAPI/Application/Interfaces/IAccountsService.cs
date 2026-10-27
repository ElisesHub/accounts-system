using AccountsAPI.Application.Dtos.Accounts;
using AccountsAPI.Application.Models;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces;

public interface IAccountsService
{
    Task<Result<AccountDto>> GetAccountAsync(string accountId);
    Task<Result<IReadOnlyList<AccountDto>?>> GetAccountsAsync();
}