
using FluentResults;
using OrchestrationApi.Application.Dtos.ApiResponses;
using OrchestrationApi.Domain.Entities;

namespace OrchestrationApi.Application.Interfaces;

public interface IAccountsService
{
    Task<Result<AccountResponse>> GetAccountAsync(int accountId, CancellationToken ct);
    Task<Result<IReadOnlyList<AccountResponse>>> GetAccountsAsync(CancellationToken ct);
}