using AccountsAPI.Presentation.Mappings;
using JetBrains.Annotations;
using Xunit;
using System;
using AwesomeAssertions;

namespace AccountsAPI.Tests.UnitTests.Presentation.Mappings;

[TestSubject(typeof(MoneyProtoMappings))]
public class MoneyProtoMappingsTests
{
    // amount, expected units, expected nanos
    public static TheoryData<decimal, long, int> Amounts => new()
    {
        { 0m,       0,    0 },
        { 5m,       5,    0 },              // no fractional part
        { 12.50m,   12,   500_000_000 },
        { -12.50m,  -12,  -500_000_000 },   // both negative
        { -0.01m,   0,    -10_000_000 },    // negative with zero units: the sign lives in nanos
        { 0.0001m,  0,    100_000 },        // the smallest amount numeric(19,4) can store
        { 999_999_999_999_999.9999m, 999_999_999_999_999, 999_900_000 }   // the largest
    };

    [Theory]
    [MemberData(nameof(Amounts))]
    public void ToMoney_splits_amount_into_units_and_nanos(decimal amount, long expectedUnits, int expectedNanos)
    {
        // Act
        var money = MoneyProtoMappings.ToMoney(amount, "EUR");

        // Assert
        money.Units.Should().Be(expectedUnits);
        money.Nanos.Should().Be(expectedNanos);
    }

    [Fact]
    public void ToMoney_sets_the_currency_code()
    {
        // Act
        var money = MoneyProtoMappings.ToMoney(1m, "GBP");

        // Assert
        money.CurrencyCode.Should().Be("GBP");
    }

    [Fact]
    public void ToMoney_throws_when_currency_code_is_null()
    {
        // Act
        Action act = () => MoneyProtoMappings.ToMoney(1m, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ToMoney_throws_when_currency_code_is_empty_or_whitespace(string currencyCode)
    {
        // Act
        Action act = () => MoneyProtoMappings.ToMoney(1m, currencyCode);

        // Assert: exactly ArgumentException, not its ArgumentNullException subclass
        act.Should().ThrowExactly<ArgumentException>();
    }
}