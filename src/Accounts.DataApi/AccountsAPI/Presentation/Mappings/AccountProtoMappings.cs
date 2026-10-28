using Accounts.Api.V1;
using AccountsAPI.Application.Dtos.Accounts;

namespace AccountsAPI.Presentation.Mappings;

/// <summary>
/// Maps application DTOs to the gRPC contract's account messages.
/// </summary>
public static class AccountProtoMappings
{
    /// <summary>
    /// Converts an <see cref="AccountDto"/> into the shared proto <see cref="Account"/> resource message.
    /// Each rpc wraps the result in its own response envelope.
    /// </summary>
    /// <param name="account">The account to convert.</param>
    /// <returns>A complete proto <see cref="Account"/>, with amounts as <c>google.type.Money</c> in the account's currency.</returns>
    public static Account ToAccount(AccountDto account) =>
        new Account
        {
            AccountId = account.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Balance = MoneyProtoMappings.ToMoney(account.Balance, account.CurrencyCode),
            OverdraftLimit = MoneyProtoMappings.ToMoney(account.OverdraftLimit, account.CurrencyCode)
        };


    public static IReadOnlyList<Account> ToAccounts(IReadOnlyList<AccountDto> accounts)
    {
        return accounts.Select(account => ToAccount(account)).ToList();
    }
}