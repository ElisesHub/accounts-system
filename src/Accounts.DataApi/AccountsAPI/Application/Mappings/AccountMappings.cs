using AccountsAPI.Application.Dtos.Accounts;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Mappings;

public static class AccountMappings
{
    public static AccountDto ToAccountDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id.Value,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Balance = account.Balance,
            OverdraftLimit = account.OverdraftLimit
        };
    }

    public static IReadOnlyList<AccountDto> ToAccountDtoList(IReadOnlyList<Account> accounts)
    {
        return accounts.Select(model => new AccountDto
        {
            Id = model.Id.Value,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Balance = model.Balance,
            OverdraftLimit = model.OverdraftLimit

        }).ToList();
    }
}