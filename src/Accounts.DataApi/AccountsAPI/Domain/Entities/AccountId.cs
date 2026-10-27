using AccountsAPI.Application.Models;

namespace AccountsAPI.Domain.Entities;

public readonly record struct AccountId
{
    public int Value { get; }

    private AccountId(int value) => Value = value;

    /// <summary>
    /// Creates an account ID after passing validation checks
    /// </summary>
    /// <param name="accountId">The ID proposal to verify</param>
    /// <returns>Returns a Result object with the AccountId object as its value property. On invalid input, the Result is a validation failure with no value.</returns>
    public static Result<AccountId> Create(int accountId)
    {
        if (accountId <= 0)
            return Result<AccountId>.Validation("accountId",
                "Account id must be a positive number");

        return Result<AccountId>.Success(new AccountId(accountId));
    }

    public override string ToString() => Value.ToString();

    /// <summary>
    /// Creates an Account ID when coming from a trusted source therefore not needing to pass validation checks
    /// </summary>
    /// <param name="value">The account ID read from a trusted source</param>
    /// <returns>Returns an account ID object</returns>
    internal static AccountId From(int value)
        => new AccountId(value);
}