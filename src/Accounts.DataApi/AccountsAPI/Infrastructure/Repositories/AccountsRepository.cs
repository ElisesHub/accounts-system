using System.Data;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Infrastructure.Mappings;
using MySqlConnector;

namespace AccountsAPI.Infrastructure.Repositories;

/// <summary>
/// Reads accounts from the MySQL database using stored procedures.
/// </summary>
public class AccountsRepository(MySqlDataSource dataSource)
    : IAccountsRepository
{
    /// <summary>
    /// Retrieves a single account by its identifier, using the <c>GetAccount</c> stored procedure.
    /// </summary>
    /// <param name="accountId">The validated ID of the account to retrieve.</param>
    /// <param name="ct">A token to cancel the operation. Cancelling also stops the running database query.</param>
    /// <returns>The <see cref="Account"/> if found, or <c>null</c> if no account has that ID.</returns>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="ct"/> is cancelled.</exception>
    public async Task<Account?> GetAccountAsync(AccountId accountId,
        CancellationToken ct)
    {
        await using var con =
            await dataSource.OpenConnectionAsync(ct);

        await using var cmd = new MySqlCommand("GetAccount", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("p_id", MySqlDbType.Int32).Value =
            accountId.Value;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return AccountDataMappings.MapFromDatabase(reader);
        }

        return null;
    }

    /// <summary>
    /// Retrieves all accounts, using the <c>GetAccounts</c> stored procedure.
    /// </summary>
    /// <param name="ct">A token to cancel the operation. Cancelling also stops the running database query.</param>
    /// <returns>The accounts. The list is empty if there are none; it is never null.</returns>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="ct"/> is cancelled.</exception>
    public async Task<IReadOnlyList<Account>> GetAccountsAsync(
        CancellationToken ct)
    {
        var list = new List<Account>();
        await using var con = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new MySqlCommand("GetAccounts", con);
        cmd.CommandType = CommandType.StoredProcedure;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(AccountDataMappings.MapFromDatabase(reader));
        }

        return list;
    }
}