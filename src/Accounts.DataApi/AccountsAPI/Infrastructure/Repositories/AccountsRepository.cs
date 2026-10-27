using System.Data;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Infrastructure.Mappings;
using MySqlConnector;

namespace AccountsAPI.Infrastructure.Repositories;

public class AccountsRepository(MySqlDataSource dataSource)
    : IAccountsRepository
{
    public async Task<Account?> GetAccountAsync(int accountId,
        CancellationToken ct)
    {
        await using var con =
            await dataSource.OpenConnectionAsync(ct);

        await using var cmd = new MySqlCommand("GetAccount", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("p_id", MySqlDbType.Int32).Value =
            accountId;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return AccountDataMappings.MapFromDatabase(reader);
        }

        return null;
    }

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