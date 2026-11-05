using FluentResults;
using OrchestrationApi.Domain.Entities;

namespace OrchestrationApi.Application.Interfaces;

public interface IExternalAccountsClient
{
    Task<Result<Account>> GetAccountAsync(int id, CancellationToken ct);
    Task<Result<IReadOnlyList<Account>>> GetAccountsAsync(CancellationToken ct);
}