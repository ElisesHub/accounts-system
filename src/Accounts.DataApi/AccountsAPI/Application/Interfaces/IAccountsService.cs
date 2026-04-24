using AccountsAPI.Application.Models;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces;

public interface IAccountsService
{
    Task<Result<Account>> GetAccountAsync(string accountId);
    Task<Result<List<Account>?>> GetAccountsAsync();
}