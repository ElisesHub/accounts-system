using AccountsAPI.Application.Interfaces;
using Accounts.Api.V1;
using AccountsAPI.Application.Models;
using AccountsAPI.Presentation.Mappings;
using Grpc.Core;

namespace AccountsAPI.Presentation.Grpc;

/// <summary>
/// gRPC implementation of the <c>accounts.v1.AccountsService</c> contract.
/// Translates application <see cref="Result{T}"/> outcomes into gRPC responses and status codes.
/// </summary>
public class AccountsGrpcService(
    ILogger<AccountsGrpcService> logger,
    IAccountsService accountsService) : AccountsService.AccountsServiceBase
{
    /// <summary>
    /// Returns a single account by its ID.
    /// </summary>
    /// <param name="request">The request containing the account ID.</param>
    /// <param name="context">The call context. Its cancellation token is passed down so a cancelled
    /// call or an expired deadline stops the database query.</param>
    /// <returns>A response containing the complete account.</returns>
    /// <exception cref="RpcException">
    /// <see cref="StatusCode.NotFound"/> if no account has the ID;
    /// <see cref="StatusCode.InvalidArgument"/> if the ID is not positive, with the field errors in the detail;
    /// <see cref="StatusCode.Internal"/> if the application returned an unexpected state (logged server-side).
    /// </exception>
    public override async Task<GetAccountResponse> GetAccount(
        GetAccountRequest request, ServerCallContext context)
    {
        var accountResult =
            await accountsService.GetAccountAsync(request.AccountId, context.CancellationToken);

        switch (accountResult.CurrentState)
        {
            case ResultStatus.NotFound:
                throw new RpcException(new Status(StatusCode.NotFound,
                    accountResult.Message ?? "Account not found"));

            case ResultStatus.ValidationError:
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    ValidationErrorFormatter.Format(accountResult.Errors)));

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