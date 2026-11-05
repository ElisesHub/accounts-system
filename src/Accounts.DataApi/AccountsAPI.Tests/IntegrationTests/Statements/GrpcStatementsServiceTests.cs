using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Grpc.Core;
using Statements.Api.V1;
using Xunit;

[assembly: Xunit.CaptureConsole]

namespace AccountsAPI.Tests.IntegrationTests.Statements;

public class GrpcStatementsServiceTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetTransactions_All_for_account_id()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var client = new StatementsService.StatementsServiceClient(factory.CreateGrpcChannel());

        // Act
        using var call = client.GetTransactions(new GetTransactionsRequest { AccountId = 1 }, headers: factory.Bearer(),
            cancellationToken: ct);

        var received = new List<Transaction>();

        await foreach (var message in call.ResponseStream.ReadAllAsync(ct))
            received.Add(message.Transaction);

        //Assert
        received.Should().HaveCount(4);
        received.Should().BeInAscendingOrder(x => x.OccurredAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task GetTransactions_returns_InvalidArgument_for_non_positive_account_id(int accountId)
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var client = new StatementsService.StatementsServiceClient(factory.CreateGrpcChannel());

        var act = async () =>
        {
            using var call = client.GetTransactions(new GetTransactionsRequest { AccountId = accountId },
                headers: factory.Bearer(),
                cancellationToken: ct);

            await foreach (var _ in call.ResponseStream.ReadAllAsync(ct))
            {
            }
        };

        // Act & Assert
        await act.Should().ThrowAsync<RpcException>().Where(e => e.StatusCode == StatusCode.InvalidArgument);
    }


    [Fact]
    public async Task GetTransactions_returns_line_fields_for_account()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var client = new StatementsService.StatementsServiceClient(factory.CreateGrpcChannel());

        // Act
        using var call = client.GetTransactions(new GetTransactionsRequest { AccountId = 1 }, headers: factory.Bearer(),
            cancellationToken: ct);

        var received = await call.ResponseStream.ReadAllAsync(ct).Select(m => m.Transaction).Take(2).ToListAsync(ct);


        //Assert
        received.Should().HaveCount(2);

        received[0].HasCounterpartyName.Should().BeFalse();
        received[0].Amount.Units.Should().Be(1500);
        received[0].Amount.Nanos.Should().Be(500_000_000);
        received[0].Amount.CurrencyCode.Should().Be("EUR");
        received[0].TransactionType.Should().Be("Opening balance");

        received[1].HasCounterpartyName.Should().BeTrue();
        received[1].CounterpartyName.Should().Be("Pies R Us");
        received[1].Amount.Units.Should().Be(-1200);
        received[1].Amount.Nanos.Should().Be(-500_000_000);
        received[1].Amount.CurrencyCode.Should().Be("EUR");
        received[1].TransactionType.Should().Be("Card payment");

    }
}