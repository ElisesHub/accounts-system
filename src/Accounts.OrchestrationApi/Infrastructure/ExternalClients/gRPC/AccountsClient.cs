using Accounts.Api.V1;
using FluentResults;
using Grpc.Core;
using Microsoft.Extensions.Options;
using PortfolioApplicationAPI.Application.Common.Errors;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Infrastructure.Mappings;
using PortfolioApplicationAPI.Infrastructure.Security.ApiKeys;
using Account = PortfolioApplicationAPI.Domain.Entities.Account;
using AccountsGrpc = Accounts.Api.V1.AccountsService;
namespace PortfolioApplicationAPI.Infrastructure.ExternalClients.gRPC;

public class AccountsClient(AccountsGrpc.AccountsServiceClient client,
    IOptions<ApiKeyOptions> options) : IExternalAccountsClient
{
    public async Task<Result<Account>> GetAccountAsync(int id, CancellationToken ct)
    {
        try
        {
            var response = await client.GetAccountAsync(
                new GetAccountRequest { AccountId = id },
                headers: new Metadata { { "x-api-key", GetOutgoingKey() } },
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

    public Task<Result<IReadOnlyList<Account>>> GetAccountsAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves the outgoing key from the configuration.
    /// </summary>
    /// <returns>A string representing the outgoing key.</returns>
    /// <exception cref="Exception">Thrown if the outgoing key is not found or is null/empty.</exception>
    private string GetOutgoingKey()
    {
        if (options?.Value is null)
        {
            throw new InvalidOperationException("ApiKeyOptions not configured.");
        }
        var key = options?.Value?.AccountsApiKey;

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Outgoing key not found.");
        }

        return key;

    }
}