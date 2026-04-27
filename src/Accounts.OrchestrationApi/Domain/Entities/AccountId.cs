namespace PortfolioApplicationAPI.Domain.Entities;
using FluentResults;
public readonly record struct AccountId
{
    public int Value { get; }

    private AccountId(int value) => Value = value;

    /// <summary>
    /// Security validation method. Creates an instance of <see cref="AccountId"/> based on the provided string input.
    /// Validates the input to ensure it is a non-empty, numeric, and positive value.
    /// </summary>
    /// <param name="accountId">The string representation of the account ID to create.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a successful result with the created <see cref="AccountId"/> instance
    /// or an appropriate validation error if the input is invalid.
    /// </returns>
    public static Result<AccountId> Create(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            return Result.Fail<AccountId>("Account id is required");

        if (!int.TryParse(accountId, out var id))
            return Result.Fail<AccountId>("Account id must be a number");

        if (id <= 0)
            return Result.Fail<AccountId>( "Account id must be a positive number");

        return Result.Ok(new AccountId(id));
    }

    public override string ToString() => Value.ToString();

    /// <summary>
    /// Creates an instance of <see cref="AccountId"/> from the provided integer value.
    /// This method is intended for internal use and does not perform validation on the value. Used for hydrating the entity from the database.
    /// </summary>
    /// <param name="value">The integer value representing the account ID.</param>
    /// <returns>
    /// An instance of <see cref="AccountId"/> encapsulating the provided value.
    /// </returns>
    internal static AccountId From(int value)
        => new AccountId(value);


}