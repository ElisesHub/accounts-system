
using FluentResults;
using PortfolioApplicationAPI.Application.Dtos.ApiResponses;
using PortfolioApplicationAPI.Domain.Entities;

namespace PortfolioApplicationAPI.Application.Interfaces;

public interface IAccountsService
{
    Task<Result<AccountResponse?>> GetAccountAsync(string accountId);
    Task<Result<IReadOnlyList<AccountResponse>>> GetAccountsAsync();
}