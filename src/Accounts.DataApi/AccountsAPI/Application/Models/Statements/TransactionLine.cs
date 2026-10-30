namespace AccountsAPI.Application.Models.Statements;

/// <summary>
/// One line of an account statement: a booked transaction as shown to the account holder.
/// A read model shaped for the statement query, not a domain entity.
/// </summary>
public sealed class TransactionLine
{

    public required long TransactionId { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>Signed: positive is money in, negative is money out.</summary>
    public required decimal Amount { get; init; }

    /// <summary>The account's ISO 4217 currency code, which applies to <see cref="Amount"/>.</summary>
    public required string CurrencyCode { get; init; }

    /// <summary>The other party, when known (for example the shop or employer).</summary>
    public string? CounterpartyName { get; init; }

    /// <summary>The transaction type's display title, for example "Card payment".</summary>
    public required string TransactionType { get; init; }

    public required string Description { get; init; }
}