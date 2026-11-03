using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Accounts.Api.V1;
using AccountsAPI.Presentation.Interceptors;
using AccountsAPI.Tests.TestHelpers;
using AwesomeAssertions;
using Grpc.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Statements.Api.V1;
using Xunit;

namespace AccountsAPI.Tests.UnitTests.Presentation.Interceptors;

[TestSubject(typeof(ExceptionInterceptor))]
public class ExceptionInterceptorTest
{
    private const string GetAccountMethod = "/accounts.v1.AccountsService/GetAccount";
    private const string GetTransactionsMethod = "/statements.v1.StatementsService/GetTransactions";
        private readonly FakeLogger<ExceptionInterceptor> logger = new();


        [Fact]
        public async Task UnaryServerHandler_RpcException_Nothing_Logged_Status_Unchanged()
        {
            //Arrange
            var exceptionInterceptor = new ExceptionInterceptor(logger);
            var context = new TestServerCallContext(GetAccountMethod, CancellationToken.None);

            UnaryServerMethod<GetAccountRequest, GetAccountResponse> continuation = (_,_) => throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

            Func<Task> act = () => exceptionInterceptor.UnaryServerHandler(new GetAccountRequest(), context, continuation);

            //Act & Assert
            var result = await act.Should().ThrowAsync<RpcException>();
            result.Which.Status.StatusCode.Should().Be(StatusCode.NotFound);
            logger.Collector.Count.Should().Be(0);

        }


    [Fact]
    public async Task UnaryServerHandler_CancelledByClient_RethrowsAndLogsWarning()
    {
        //Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var exceptionInterceptor = new ExceptionInterceptor(logger);
        var context = new TestServerCallContext(GetAccountMethod, cts.Token);

        UnaryServerMethod<GetAccountRequest, GetAccountResponse> continuation = (_,ctx) => throw new OperationCanceledException(ctx.CancellationToken);

        Func<Task> act = () => exceptionInterceptor.UnaryServerHandler(new GetAccountRequest(), context, continuation);

        //Act & Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        logger.Collector.Count.Should().Be(1);
        logger.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }

    [Fact] public async Task UnaryServerHandler_CancelledInternally_ThrowsInternalAndLogsError()
    {
            //Arrange
            using var internalCts = new CancellationTokenSource();
            await internalCts.CancelAsync();

            var exceptionInterceptor = new ExceptionInterceptor(logger);
            var context = new TestServerCallContext(GetAccountMethod, CancellationToken.None);

            UnaryServerMethod<GetAccountRequest, GetAccountResponse> continuation = (_,_) => throw new OperationCanceledException(internalCts.Token);

            Func<Task> act = () => exceptionInterceptor.UnaryServerHandler(new GetAccountRequest(), context, continuation);

            //Act & Assert
            var result = await act.Should().ThrowAsync<RpcException>();
            result.Which.Status.StatusCode.Should().Be(StatusCode.Internal);
            result.Which.Status.Detail.Should().Contain("Trace ID");
            logger.Collector.Count.Should().Be(1);
            logger.LatestRecord.Level.Should().Be(LogLevel.Error);

    }



    [Fact] public async Task UnaryServerHandler_InvalidOperationException_ThrowsInternalAndLogsError()
    {
        //Arrange
        var exceptionInterceptor = new ExceptionInterceptor(logger);
        var context = new TestServerCallContext(GetAccountMethod, CancellationToken.None);

        UnaryServerMethod<GetAccountRequest, GetAccountResponse> continuation = (_,_) => throw new InvalidOperationException();

        Func<Task> act = () => exceptionInterceptor.UnaryServerHandler(new GetAccountRequest(), context, continuation);

        //Act & Assert
        var result = await act.Should().ThrowAsync<RpcException>();
        result.Which.Status.StatusCode.Should().Be(StatusCode.Internal);
        result.Which.Status.Detail.Should().Contain("Trace ID");
        logger.Collector.Count.Should().Be(1);
        logger.LatestRecord.Level.Should().Be(LogLevel.Error);

    }


    [Fact] public async Task UnaryServerHandler_ReturnsSuccess()
    {
        //Arrange
        var exceptionInterceptor = new ExceptionInterceptor(logger);
        var context = new TestServerCallContext(GetAccountMethod, CancellationToken.None);
var response = new GetAccountResponse();
        UnaryServerMethod<GetAccountRequest, GetAccountResponse> continuation = (_,_) => Task.FromResult(response);
        var result  = await exceptionInterceptor.UnaryServerHandler(new GetAccountRequest(), context, continuation);

        //Act & Assert
       result.Should().BeSameAs(response);
        logger.Collector.Count.Should().Be(0);

    }



[Fact] public async Task ServerStreamingServerHandler_UnexpectedException_ThrowsInternalAndLogsError()
{
    //Arrange
    var exceptionInterceptor = new ExceptionInterceptor(logger);
    var responseStream = Substitute.For<IServerStreamWriter<GetTransactionsResponse>>();
    var context = new TestServerCallContext(GetTransactionsMethod, CancellationToken.None);

    ServerStreamingServerMethod<GetTransactionsRequest, GetTransactionsResponse> continuation = (_,_,_) => throw new EndOfStreamException();


    Func<Task> act  = () => exceptionInterceptor.ServerStreamingServerHandler(new GetTransactionsRequest(), responseStream, context, continuation);

    //Act & Assert
    var result = await act.Should().ThrowAsync<RpcException>();
    result.Which.Status.StatusCode.Should().Be(StatusCode.Internal);
    result.Which.Status.Detail.Should().Contain("Trace ID");
    logger.Collector.Count.Should().Be(1);
    logger.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}