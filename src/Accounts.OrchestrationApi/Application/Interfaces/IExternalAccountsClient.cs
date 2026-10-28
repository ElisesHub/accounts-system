using FluentResults;
using PortfolioApplicationAPI.Domain.Entities;

namespace PortfolioApplicationAPI.Application.Interfaces;

public interface IExternalAccountsClient
{
    Task<Result<Account>> GetAccountAsync(int id, CancellationToken ct);
    Task<Result<IReadOnlyList<Account>>> GetAccountsAsync(CancellationToken ct);
}