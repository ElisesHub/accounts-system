using AccountsAPI.Application.Interfaces;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Infrastructure.Mappings;
using Npgsql;

namespace AccountsAPI.Infrastructure.Repositories;

/// <summary>
/// Reads accounts from the PostgreSQL database using database functions.
/// </summary>
public class AccountsRepository(NpgsqlDataSource dataSource)
    : IAccountsRepository
{
    // Postgres functions are queried like tables, not called as stored procedures.
    // Columns are listed explicitly so a change to the function's output can't silently reorder them.
    private const string GetAccountSql =
        """
        SELECT account_id, first_name, last_name, balance, overdraft_limit, currency_code
         FROM accounts a
                 JOIN customers c ON c.customer_id = a.customer_id
        WHERE a.account_id = @account_id;
        """;

    private const string GetAccountsSql =
        """
        SELECT account_id, first_name, last_name, balance, overdraft_limit, currency_code
        FROM accounts a JOIN customers c ON c.customer_id = a.customer_id
        ORDER BY a.account_id;
        """;

    /// <summary>
    /// Retrieves a single account by its identifier, using the <c>get_account</c> function.
    /// </summary>
    /// <param name="accountId">The validated ID of the account to retrieve.</param>
    /// <param name="ct">A token to cancel the operation. Cancelling also stops the running database query.</param>
    /// <returns>The <see cref="Account"/> if found, or <c>null</c> if no account has that ID.</returns>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="ct"/> is cancelled.</exception>
    public async Task<Account?> GetAccountAsync(AccountId accountId,
        CancellationToken ct)
    {
        await using var con = await dataSource.OpenConnectionAsync(ct);

        await using var cmd = new NpgsqlCommand(GetAccountSql, con);

        cmd.Parameters.Add(new NpgsqlParameter<int>("account_id", accountId.Value));

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return AccountDataMappings.MapFromDatabase(reader);
        }

        return null;
    }

    /// <summary>
    /// Retrieves all accounts, using the <c>get_accounts</c> function.
    /// </summary>
    /// <param name="ct">A token to cancel the operation. Cancelling also stops the running database query.</param>
    /// <returns>The accounts. The list is empty if there are none; it is never null.</returns>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="ct"/> is cancelled.</exception>
    public async Task<IReadOnlyList<Account>> GetAccountsAsync(
        CancellationToken ct)
    {
        var list = new List<Account>();

        await using var con = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(GetAccountsSql, con);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(AccountDataMappings.MapFromDatabase(reader));
        }

        return list;
    }
}