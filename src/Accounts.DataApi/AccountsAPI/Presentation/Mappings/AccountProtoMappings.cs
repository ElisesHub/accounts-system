using Accounts.Api.V1;
using AccountsAPI.Application.Dtos.Accounts;

namespace AccountsAPI.Presentation.Mappings;

public static class AccountProtoMappings // `static class`, the same as MoneyProtoMappings.
{
    public static Account ToAccount(AccountDto account) =>
         new Account
        {
            AccountId = account.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email,
            Balance = MoneyProtoMappings.ToMoney(account.Balance, account.CurrencyCode),
            OverdraftLimit = MoneyProtoMappings.ToMoney(account.OverdraftLimit, account.CurrencyCode)
        };

}