using System.Data.Common;
using AccountsAPI.Domain.Entities;

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
    public static Account MapFromDatabase(DbDataReader reader) => new Account
    {
        Id = AccountId.From(reader.GetInt32(reader.GetOrdinal("account_id"))),
        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
        LastName = reader.GetString(reader.GetOrdinal("last_name")),
        Balance = reader.GetDecimal(reader.GetOrdinal("balance")),
        OverdraftLimit = reader.GetDecimal(reader.GetOrdinal("overdraft_limit"))
    };
}