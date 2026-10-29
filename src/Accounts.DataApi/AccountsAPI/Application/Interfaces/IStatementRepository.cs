using AccountsAPI.Domain.Entities;
using AccountsAPI.Domain.Statements;

namespace AccountsAPI.Application.Interfaces;

public interface IStatementRepository
{
    IAsyncEnumerable<TransactionLine> StreamTransactionsAsync(
        AccountId accountId,  CancellationToken ct);
}