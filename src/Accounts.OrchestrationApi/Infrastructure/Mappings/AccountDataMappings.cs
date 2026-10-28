

using PortfolioApplicationAPI.Domain.Entities;
namespace PortfolioApplicationAPI.Infrastructure.Mappings;
using GrpcModels = Accounts.Api.V1;

public static class AccountDataMappings
{
    public static Account MapToAccount(GrpcModels.Account? account)
    {
        if (account is null) throw new InvalidOperationException("Data API returned a null account");
        if (account.Balance is null) throw new InvalidOperationException($"A null account balance was spotted for account ID {account.AccountId}");
        if (account.OverdraftLimit is null) throw new InvalidOperationException($"A null overdraft limit was spotted for account ID {account.AccountId}");

       return new Account
        {
            Id = AccountId.From(account.AccountId),
            FirstName = account.FirstName,
            LastName = account.LastName,
            Balance = MoneyMappings.ToDecimal(account.Balance),
            OverdraftLimit = MoneyMappings.ToDecimal(account.OverdraftLimit),
            CurrencyCode = account.Balance.CurrencyCode

        };
    }

    public static IReadOnlyList<Account> ToAccounts(
        IReadOnlyList<GrpcModels.Account> accounts)
    {
        return accounts.Select(account => MapToAccount(account)).ToList();
    }
}