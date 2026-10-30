using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Interfaces.Repositories;
using AccountsAPI.Application.Models;
using AccountsAPI.Application.Services;
using AccountsAPI.Application.Services.Accounts;
using AccountsAPI.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountsAPI.Tests.UnitTests.Application.Services;

public class AccountsServiceTests
{
    [Fact]
    public async Task GetAccount_returns_single_account()
    {
        // Arrange
        var returnedAccount = new Account
        {
            Id = AccountId.Create(1).Value,
            FirstName = "John",
            LastName = "Smith",
            Balance = 1500.50m,
            OverdraftLimit = 500.00m
        };

        var accountsRepo = Substitute.For<IAccountsRepository>();
        accountsRepo.GetAccountAsync(AccountId.Create(1).Value, Arg.Any<CancellationToken>()).Returns(returnedAccount);

        var service = new AccountsService(accountsRepo);

        // Act
        var result = await service.GetAccountAsync(1, TestContext.Current.CancellationToken);

        // Assert
        result.CurrentState.Should().Be(ResultStatus.Success);

        result.Value.Should().BeEquivalentTo(new
        {
            Id = 1,
            FirstName = "John",
            LastName = "Smith",
            Balance = 1500.50m,
            OverdraftLimit = 500.00m
        });
    }

    [Fact]
    public async Task GetAccounts_returns_list_of_accounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new()
            {
                Id = AccountId.Create(1).Value,
                FirstName = "John",
                LastName = "Smith",
                Balance = 1500.50m,
                OverdraftLimit = 500.00m
            },
            new()
            {
                Id = AccountId.Create(2).Value,
                FirstName = "Jane",
                LastName = "Smith",
                Balance = 1000.50m,
                OverdraftLimit = 700.00m
            }
        };

        var accountsRepo = Substitute.For<IAccountsRepository>();
        accountsRepo.GetAccountsAsync(Arg.Any<CancellationToken>()).Returns(accounts);

        var service = new AccountsService(accountsRepo);

        // Act
        var result = await service.GetAccountsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.CurrentState.Should().Be(ResultStatus.Success);
        result.Errors.Should().BeNullOrEmpty();
        result.Value.Should().BeEquivalentTo([
            new { Id = 1, FirstName = "John", LastName = "Smith", Balance = 1500.50m, OverdraftLimit = 500.00m },
            new { Id = 2, FirstName = "Jane", LastName = "Smith", Balance = 1000.50m, OverdraftLimit = 700.00m }
        ]);
    }
}