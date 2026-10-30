using System.Data.Common;
using System.Transactions;
using AccountsAPI.Application.Models.Statements;
using Google.Protobuf.WellKnownTypes;
using grpcStatements = Statements.Api.V1;
namespace AccountsAPI.Presentation.Mappings;

public static class StatementProtoMappings
{
    public static grpcStatements.Transaction ToTransaction(TransactionLine t)
    {
        return new grpcStatements.Transaction
        {
            TransactionId = t.TransactionId,
            OccurredAt = Timestamp.FromDateTimeOffset(t.OccurredAt),
            Amount = MoneyProtoMappings.ToMoney(t.Amount, t.CurrencyCode),
            CounterpartyName = t.CounterpartyName,
            TransactionType = t.TransactionType,
            Description = t.Description
        };
    }
}