namespace AccountsAPI.Domain.Entities;

public sealed class Account
{
    public required AccountId Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required decimal Balance { get; set; }
    public required decimal OverdraftLimit { get; set; }
}