
using AccountsApplicationAPI.Application.Interfaces;
using FluentResults;
using PortfolioApplicationAPI.Application.Common.Errors;
using PortfolioApplicationAPI.Application.Dtos.ApiResponses;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Application.Mappings;
using PortfolioApplicationAPI.Domain.Entities;

namespace AccountsAPI.Services;

public class AccountsService(IExternalAccountsClient externalAccountsClient ) : IAccountsService
{
    public async Task<Result<AccountResponse?>> GetAccountAsync(string accountId)
    {
        var account = await externalAccountsClient.GetAccountAsync(accountId);
        if(account == null) throw new Exception("Account not found");

        if (account.Id.Value <= 0 || account.Id == default)
            return Result.Fail<AccountResponse?>(new NotFoundError("No account found"));

        return Result.Ok(AccountMappings.ToResponse(account));

    }

    public async Task<Result<IReadOnlyList<AccountResponse>>> GetAccountsAsync()
    {
        var accountList = await externalAccountsClient.GetAccountsAsync();

        if (accountList is  null || !accountList.Any())
            return Result.Fail("No accounts found");

        return Result.Ok(AccountMappings.ToResponse(accountList));
    }


}