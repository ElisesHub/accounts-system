using AccountsAPI.Application.Dtos.Accounts;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Mappings;

public static class AccountMappings
{
    public static AccountDto ToResponse(Account account)
    {
        return new AccountDto
        {
            Id = account.Id.Value,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email,
            Balance = account.Balance,
            OverdraftLimit = account.OverdraftLimit
        };
    }

    public static IReadOnlyList<AccountDto> ToResponseList(IReadOnlyList<Account> accounts)
    {
        return accounts.Select(model => new AccountDto
        {
            Id = model.Id.Value,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Balance = model.Balance,
            OverdraftLimit = model.OverdraftLimit

        }).ToList();
    }
}