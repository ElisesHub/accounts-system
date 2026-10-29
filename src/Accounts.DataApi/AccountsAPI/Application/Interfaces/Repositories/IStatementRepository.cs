using AccountsAPI.Application.Models.Statements;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Interfaces.Repositories;

public interface IStatementRepository
{
    IAsyncEnumerable<TransactionLine> StreamTransactionsAsync(
        AccountId accountId,  CancellationToken ct);
}