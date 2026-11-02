using System;
using AccountsAPI.Application.Dtos.Accounts;
using AccountsAPI.Presentation.Mappings;
using AwesomeAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AccountsAPI.Tests.Presentation.Mappings;

[TestSubject(typeof(AccountProtoMappings))]
public class AccountProtoMappingsTest
{
    [Fact]
    public void To_proto_account_valid_data()
    {
        //Arrange
        var data = new AccountDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Delaneyy",
            Balance = -100.10m,
            OverdraftLimit = 125.4m
        };

        //Act
        var conversion = data.ToProtoAccount();

        //Assert
        conversion.Should().NotBeNull();

        conversion.Balance.Units.Should().Be(-100);
        conversion.Balance.Nanos.Should().Be(-100_000_000);
        conversion.Balance.CurrencyCode.Should().Be("EUR");

        conversion.OverdraftLimit.Units.Should().Be(125);
        conversion.OverdraftLimit.Nanos.Should().Be(400_000_000);
        conversion.OverdraftLimit.CurrencyCode.Should().Be("EUR");

        conversion.FirstName.Should().Be("John");
        conversion.LastName.Should().Be("Delaneyy");
        conversion.AccountId.Should().Be(1);
    }


}