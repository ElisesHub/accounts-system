namespace AccountsAPI.Domain.Statements;

public sealed class TransactionLine
{
    public required long TransactionId { get; init; }
    /// <summary>When the transaction happened (UTC).</summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>Signed: positive is money in, negative is money out. In the account's currency.</summary>
    public required decimal Amount { get; init; }

    public string? CounterpartyName { get; init; }

    public required string TransactionType { get; init; }

    public required string Description { get; init; }

}