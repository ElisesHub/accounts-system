using AccountsAPI.Application.Dtos.ApiResponses;
using AccountsAPI.Application.Models;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces;

public interface IAccountsService
{
    Task<Result<AccountResponse>> GetAccountAsync(string accountId);
    Task<Result<IReadOnlyList<AccountResponse>?>> GetAccountsAsync();
}