using FluentResults;
using PortfolioApplicationAPI.Domain.Entities;

namespace AccountsApplicationAPI.Application.Interfaces;

public interface IExternalAccountsClient
{
    Task<Account?> GetAccountAsync(string id);
    Task<IReadOnlyList<Account>?> GetAccountsAsync();
}