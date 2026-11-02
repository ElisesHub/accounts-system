
using AccountsAPI.Application.Interfaces.Repositories;
using AccountsAPI.Application.Interfaces.Services;
using AccountsAPI.Application.Models;
using AccountsAPI.Application.Models.Statements;
using AccountsAPI.Domain.Entities;
namespace AccountsAPI.Application.Services.Statements;

public class StatementsService(IAccountsRepository accountsRepository, IStatementRepository statementsRepository) : IStatementsService
{
    public async Task<Result<IAsyncEnumerable<TransactionLine>>> StreamTransactionsAsync(AccountId accountId, CancellationToken ct)
    {
        var accountExists = await accountsRepository.AccountExistsAsync(accountId, ct);

        if (!accountExists)
            return Result<IAsyncEnumerable<TransactionLine>>.NotFound($"Account {accountId.Value} was not found.");

        var transactions = statementsRepository.StreamTransactionsAsync(accountId, ct);

        return Result<IAsyncEnumerable<TransactionLine>>.Success(transactions);
    }
}