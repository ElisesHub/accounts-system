using System.Runtime.CompilerServices;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Interfaces.Repositories;
using AccountsAPI.Application.Models.Statements;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Infrastructure.Mappings;
using Npgsql;

namespace AccountsAPI.Infrastructure.Repositories;

public class StatementRepository(NpgsqlDataSource dataSource)
    : IStatementRepository
{
    private const string GetTransactionSqlString = """
                                                    select t.transaction_id, amount, occurred_at, counterparty_name, description, tt.title as transaction_type, a.currency_code from transactions t join transaction_types tt on  t.transaction_type_id = tt.transaction_type_id 
                                                   join accounts a on t.account_id = a.account_id           where  t.account_id = @account_id order by t.account_id, occurred_at, t.transaction_id
                                                               /* order by matches the index for faster results*/
                                                   """;

    public async IAsyncEnumerable<TransactionLine> StreamTransactionsAsync(
        AccountId accountId, [EnumeratorCancellation] CancellationToken ct)
    {
        await using var con = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(GetTransactionSqlString, con);
        cmd.Parameters.Add(
            new NpgsqlParameter<int>("account_id", accountId.Value));

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            yield return StatementDataMappings.MapTransactionLineFromDatabase(
                reader);
        }
    }
}