using System.Data.Common;
using System.Transactions;
using AccountsAPI.Application.Models.Statements;
using Google.Protobuf.WellKnownTypes;
using grpcStatements = Statements.Api.V1;
namespace AccountsAPI.Presentation.Mappings;

/// <summary>
/// Maps statement read models to their gRPC contract messages.
/// </summary>
public static class StatementProtoMappings
{
    /// <summary>
    /// Converts a <see cref="TransactionLine"/> to the proto <c>Transaction</c> message.
    /// The counterparty is only set when present: protobuf strings cannot be null, and leaving
    /// the optional field unset lets clients tell "no counterparty" from an empty name.
    /// </summary>
    /// <param name="t">The transaction line to convert.</param>
    /// <returns>The proto message for the transaction line.</returns>
    public static grpcStatements.Transaction ToTransaction(TransactionLine t)
    {
        var transaction = new grpcStatements.Transaction
        {
            TransactionId = t.TransactionId,
            OccurredAt = Timestamp.FromDateTimeOffset(t.OccurredAt),
            Amount = MoneyProtoMappings.ToMoney(t.Amount, t.CurrencyCode),
            TransactionType = t.TransactionType,
            Description = t.Description
        };

        if (t.CounterpartyName is not null)
            transaction.CounterpartyName = t.CounterpartyName;

        return transaction;
    }
}