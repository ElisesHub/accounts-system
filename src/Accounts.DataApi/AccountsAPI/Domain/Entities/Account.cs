namespace AccountsAPI.Domain.Entities;

public sealed class Account
{
    public required AccountId Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required decimal Balance { get; init; }
    public required decimal OverdraftLimit { get; init; }
}