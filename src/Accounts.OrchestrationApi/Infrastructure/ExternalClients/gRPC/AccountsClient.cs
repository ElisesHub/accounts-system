using Accounts.Api.V1;
using FluentResults;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using PortfolioApplicationAPI.Application.Common.Errors;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Infrastructure.Mappings;
using PortfolioApplicationAPI.Infrastructure.Security.ApiKeys;
using PortfolioApplicationAPI.Presentation.Models;
using Account = PortfolioApplicationAPI.Domain.Entities.Account;
using AccountsGrpc = Accounts.Api.V1.AccountsService;
namespace PortfolioApplicationAPI.Infrastructure.ExternalClients.gRPC;

public class AccountsClient(AccountsGrpc.AccountsServiceClient client) : IExternalAccountsClient
{
    public async Task<Result<Account>> GetAccountAsync(int id, CancellationToken ct)
    {
        try
        {
            var response = await client.GetAccountAsync(
                new GetAccountRequest { AccountId = id },
                deadline: DateTime.UtcNow.AddSeconds(5),
                cancellationToken: ct);

            return Result.Ok(
                AccountDataMappings.MapToAccount(response.Account));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return Result.Fail<Account>(new NotFoundError(ex.Status.Detail));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
        {
            return Result.Fail<Account>(new ValidationError(ex.Status.Detail));
        }

    }

    public async Task<Result<IReadOnlyList<Account>>> GetAccountsAsync(CancellationToken ct)
    {
        var response = await client.GetAccountsAsync(
            new GetAccountsRequest{},
            deadline: DateTime.UtcNow.AddSeconds(5),
            cancellationToken: ct);

        return Result.Ok<IReadOnlyList<Account>>(AccountDataMappings.ToAccounts(response.Accounts));
    }


}