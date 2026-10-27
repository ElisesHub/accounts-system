namespace AccountsAPI.Application.Dtos.Accounts;

public sealed record AccountDto
{
    public required int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required decimal Balance { get; init; }
    public required decimal OverdraftLimit { get; init; }

    public string CurrencyCode => "EUR"; // TODO: Add different currency types later
}