using AccountsAPI.Application.Dtos.ApiResponses;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Mappings;

public static class AccountMappings
{
    public static AccountResponse ToResponse(Account account)
    {
        return new AccountResponse
        {
            Id = account.Id.Value,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email,
            Balance = account.Balance,
            OverdraftLimit = account.OverdraftLimit
        };
    }

    public static IReadOnlyList<AccountResponse> ToResponseList(IReadOnlyList<Account> accounts)
    {
        return accounts.Select(model => new AccountResponse
        {
            Id = model.Id.Value,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Balance = model.Balance,
            OverdraftLimit = model.OverdraftLimit

        }).ToList();
    }
}