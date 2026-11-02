using System.Data.Common;
using AccountsAPI.Application.Models.Statements;

namespace AccountsAPI.Infrastructure.Mappings;

public static class StatementDataMappings
{
    public static TransactionLine MapTransactionLineFromDatabase(
        DbDataReader reader) =>
        new()
        {
            TransactionId = reader.GetFieldValue<long>(reader.GetOrdinal("transaction_id")),
            OccurredAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("occurred_at")),
            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
            Description = reader.GetString(reader.GetOrdinal("description")),
            CounterpartyName = !reader.IsDBNull(reader.GetOrdinal("counterparty_name"))
                ? reader.GetString(reader.GetOrdinal("counterparty_name"))
                : null,
            TransactionType = reader.GetString(reader.GetOrdinal("transaction_type")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("currency_code"))
        };
}