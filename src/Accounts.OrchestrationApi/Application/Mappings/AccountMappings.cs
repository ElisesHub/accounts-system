using PortfolioApplicationAPI.Application.Dtos.ApiResponses;
using PortfolioApplicationAPI.Domain.Entities;

namespace PortfolioApplicationAPI.Application.Mappings;

public static class AccountMappings
{
    public static AccountResponse ToResponse(Account account)
    {
        return new AccountResponse()
        {
            Id = account.Id.Value,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email,
            Balance = account.Balance,
            OverdraftLimit = account.OverdraftLimit
        };
    }

    public static IReadOnlyList<AccountResponse> ToResponse(
         IReadOnlyList<Account> accounts)
    {
        return accounts.Select(model => new AccountResponse()
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