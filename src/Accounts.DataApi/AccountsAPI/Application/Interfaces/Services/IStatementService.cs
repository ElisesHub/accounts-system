using AccountsAPI.Application.Models;
using AccountsAPI.Application.Models.Statements;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces.Services;

public interface IStatementsService
{
    Task<Result<IAsyncEnumerable<TransactionLine>>> StreamTransactionsAsync(AccountId accountId, CancellationToken ct);
}