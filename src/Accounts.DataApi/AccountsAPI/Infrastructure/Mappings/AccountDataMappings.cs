using AccountsAPI.Domain.Entities;
using MySqlConnector;

namespace AccountsAPI.Infrastructure.Mappings;

public static class AccountDataMappings
{
    public static Account MapFromDatabase(MySqlDataReader reader) => new Account
    {
        Id = AccountId.From(reader.GetInt32(reader.GetOrdinal("id"))),
        FirstName = reader.GetString(reader.GetOrdinal("firstname")),
        LastName = reader.GetString(reader.GetOrdinal("lastname")),
        Balance = reader.GetDecimal(reader.GetOrdinal("balance")),
        OverdraftLimit = reader.GetDecimal(reader.GetOrdinal("overdraftlimit"))
    };
}