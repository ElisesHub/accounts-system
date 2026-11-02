using System;
using AccountsAPI.Application.Models.Statements;
using AccountsAPI.Presentation.Mappings;
using AwesomeAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AccountsAPI.Tests.UnitTests.Presentation.Mappings;

[TestSubject(typeof(StatementProtoMappings))]
public class StatementProtoMappingsTest{

    [Fact]
    public void ToProtoTransaction_maps_all_fields()
    {
        // Arrange
        var line = CreateLine(counterpartyName: "Pies R Us");

        // Act
        var proto = line.ToProtoTransaction();

        // Assert
        proto.TransactionId.Should().Be(42);
        proto.OccurredAt.ToDateTimeOffset().Should().Be(line.OccurredAt);
        proto.Amount.Units.Should().Be(-1200);
        proto.Amount.Nanos.Should().Be(-500_000_000);
        proto.Amount.CurrencyCode.Should().Be("EUR");
        proto.TransactionType.Should().Be("Card payment");
        proto.Description.Should().Be("Card payment");
        proto.HasCounterpartyName.Should().BeTrue();
        proto.CounterpartyName.Should().Be("Pies R Us");
    }

    [Fact]
    public void ToProtoTransaction_leaves_counterparty_unset_when_null()
    {
        // Arrange
        var line = CreateLine(counterpartyName: null);

        // Act
        var proto = line.ToProtoTransaction();

        // Assert: unset, not an empty string, and no exception from the null
        proto.HasCounterpartyName.Should().BeFalse();
    }


    private static TransactionLine CreateLine(string? counterpartyName) => new()
    {
        TransactionId = 42,
        OccurredAt = new DateTimeOffset(2026, 1, 1, 10, 15, 0, TimeSpan.Zero),
        Amount = -1200.50m,
        CurrencyCode = "EUR",
        CounterpartyName = counterpartyName,
        TransactionType = "Card payment",
        Description = "Card payment"
    };
}