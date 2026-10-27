using AccountsAPI.Application.Interfaces;
using Accounts.Api.V1;
using AccountsAPI.Application.Models;
using AccountsAPI.Presentation.Mappings;
using Grpc.Core;

namespace AccountsAPI.Presentation.Grpc;

public class AccountsGrpcService(
    ILogger<AccountsGrpcService> logger,
    IAccountsService accountsService) : AccountsService.AccountsServiceBase
{
    public override async Task<GetAccountResponse> GetAccount(
        GetAccountRequest request, ServerCallContext context)
    {
        var accountResult =
            await accountsService.GetAccountAsync(request.AccountId
                .ToString());

        switch (accountResult.CurrentState)
        {
            case ResultStatus.NotFound:
                throw new RpcException(new Status(StatusCode.NotFound,
                    accountResult.Message ?? "Account not found"));

            case ResultStatus.ValidationError:
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"{ValidationErrorFormatter.Format(accountResult.Errors)}"));

            case ResultStatus.Success:
                return new GetAccountResponse
                {
                    Account =
                        AccountProtoMappings.ToAccount(accountResult.Value!)
                };

            case ResultStatus.Unknown:
                logger.LogError(
                    "Result for account {AccountId} in {Method} had no state set ({State})",
                    request.AccountId, context.Method, accountResult.CurrentState);
                throw new RpcException(new Status(StatusCode.Internal,
                    "An internal error occurred"));

            default:
                logger.LogError(
                    "Result for account {AccountId} in {Method} found an unexpected state ({State})",
                    request.AccountId, context.Method, accountResult.CurrentState);
                throw new RpcException(new Status(StatusCode.Internal,
                    "An internal error occurred"));
        }
    }
}