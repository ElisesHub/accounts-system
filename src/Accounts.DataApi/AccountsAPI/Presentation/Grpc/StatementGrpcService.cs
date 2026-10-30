using System.Diagnostics;

using AccountsAPI.Application.Interfaces.Services;
using AccountsAPI.Application.Models;
using AccountsAPI.Presentation.Mappings;
using Grpc.Core;
using grpcService =  Statements.Api.V1;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Presentation.Helpers;

namespace AccountsAPI.Presentation.Grpc;

public class StatementGrpcService(ILogger<StatementGrpcService> logger, IStatementsService statementService) : grpcService.StatementsService.StatementsServiceBase
{
    public override async Task GetTransactions(grpcService.GetTransactionsRequest request, IServerStreamWriter<grpcService.GetTransactionsResponse> responseStream, ServerCallContext context)
    {
        var accountIdResult = AccountId.Create(request.AccountId);

        var accountId = accountIdResult.GetValueOrThrow(logger, "Account ID", request.AccountId.ToString());

        var result = await statementService.StreamTransactionsAsync(accountId, context.CancellationToken);

        var transactionEnumerator = result.GetValueOrThrow(logger, "Account ID", accountId.ToString() );

        await foreach (var transaction in transactionEnumerator)
        {
            var response = new grpcService.GetTransactionsResponse
            {
                Transaction = StatementProtoMappings.ToTransaction(transaction)
            };
            await responseStream.WriteAsync(response);
        }

    }

}