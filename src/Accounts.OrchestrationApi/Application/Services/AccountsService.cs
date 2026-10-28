using FluentResults;
using PortfolioApplicationAPI.Application.Common.Errors;
using PortfolioApplicationAPI.Application.Dtos.ApiResponses;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Application.Mappings;

namespace PortfolioApplicationAPI.Application.Services;

public class AccountsService(IExternalAccountsClient externalAccountsClient, ILogger<AccountsService> logger) : IAccountsService
{
    public async Task<Result<AccountResponse>> GetAccountAsync(int accountId, CancellationToken ct)
    {
        if(accountId < 1) return Result.Fail<AccountResponse>(new ValidationError("Invalid account id provided"));

        var accountResult = await externalAccountsClient.GetAccountAsync(accountId, ct);

        if(accountResult.IsFailed) return Result.Fail<AccountResponse>(accountResult.Errors);

        var account = accountResult.Value;

        return Result.Ok(AccountMappings.ToResponse(account));

    }

    public async Task<Result<IReadOnlyList<AccountResponse>>> GetAccountsAsync(CancellationToken ct)
    {
        var accountListResult = await externalAccountsClient.GetAccountsAsync(ct);

        if(accountListResult.IsFailed) return Result.Fail<IReadOnlyList<AccountResponse>>(accountListResult.Errors);

        var accountList = accountListResult.Value;
        return Result.Ok(AccountMappings.ToResponse(accountList));
    }


}