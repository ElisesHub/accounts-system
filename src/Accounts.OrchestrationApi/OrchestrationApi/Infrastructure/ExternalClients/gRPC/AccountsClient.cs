using Accounts.Api.V1;
using FluentResults;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using OrchestrationApi.Application.Common.Errors;
using OrchestrationApi.Application.Interfaces;
using OrchestrationApi.Infrastructure.Mappings;
using OrchestrationApi.Infrastructure.Security.ApiKeys;
using OrchestrationApi.Presentation.Models;
using Account = OrchestrationApi.Domain.Entities.Account;
using AccountsGrpc = Accounts.Api.V1.AccountsService;
using Entities_Account = OrchestrationApi.Domain.Entities.Account;

namespace OrchestrationApi.Infrastructure.ExternalClients.gRPC;

public class AccountsClient(AccountsGrpc.AccountsServiceClient client) : IExternalAccountsClient
{
    public async Task<Result<Entities_Account>> GetAccountAsync(int id, CancellationToken ct)
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
            return Result.Fail<Entities_Account>(new NotFoundError(ex.Status.Detail));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
        {
            return Result.Fail<Entities_Account>(new ValidationError(ex.Status.Detail));
        }

    }

    public async Task<Result<IReadOnlyList<Entities_Account>>> GetAccountsAsync(CancellationToken ct)
    {
        var response = await client.GetAccountsAsync(
            new GetAccountsRequest{},
            deadline: DateTime.UtcNow.AddSeconds(5),
            cancellationToken: ct);

        return Result.Ok<IReadOnlyList<Entities_Account>>(AccountDataMappings.ToAccounts(response.Accounts));
    }


}