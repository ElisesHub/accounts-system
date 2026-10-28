using AccountsAPI.Domain.Entities;
using MySqlConnector;

namespace AccountsAPI.Infrastructure.Mappings;

/// <summary>
/// Maps rows read from the accounts database to domain entities.
/// </summary>
public static class AccountDataMappings
{
    /// <summary>
    /// Creates an <see cref="Account"/> from the current row of <paramref name="reader"/>.
    /// The data is treated as trusted, so the ID is rehydrated with <see cref="AccountId.From"/>
    /// rather than validated.
    /// </summary>
    /// <param name="reader">A reader positioned on a row containing the account columns.</param>
    /// <returns>The <see cref="Account"/> for the current row.</returns>
    public static Account MapFromDatabase(MySqlDataReader reader) => new Account
    {
        Id = AccountId.From(reader.GetInt32(reader.GetOrdinal("id"))),
        FirstName = reader.GetString(reader.GetOrdinal("firstname")),
        LastName = reader.GetString(reader.GetOrdinal("lastname")),
        Balance = reader.GetDecimal(reader.GetOrdinal("balance")),
        OverdraftLimit = reader.GetDecimal(reader.GetOrdinal("overdraftlimit"))
    };
}