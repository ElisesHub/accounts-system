using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Services;
using AccountsAPI.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AccountsAPI.Tests.UnitTests.Application.Services;

public class AccountsServiceTests
{
    [Fact]
    public async Task GetAccount_returns_single_account()
    {
        // Arrange
        var returnedAccount = new Account()
        {
            Id= AccountId.Create("1").Value,
            FirstName = "John",
            LastName = "Smith",
            Balance = 1500.50m,
            OverdraftLimit = 500.00m
        };

        var accountsRepo = new Mock<IAccountsRepository>();
        accountsRepo
            .Setup(r => r.GetAccountAsync("1"))
            .ReturnsAsync(returnedAccount);

        var service = new AccountsService(accountsRepo.Object);

        // Act
        var result = await service.GetAccountAsync("1");

        //Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(returnedAccount);

    }
    [Fact]
    public async Task GetAccounts_returns_list_of_accounts()
    {
        // Arrange
        var accounts = new List<Account>()
        {
            new()
            {
                Id=  AccountId.Create("1").Value,
                FirstName = "John",
                LastName = "Smith",
                Balance = 1500.50m,
                OverdraftLimit = 500.00m
            },
            new()
            {
                Id = AccountId.Create("2").Value,
                FirstName = "Jane",
                LastName = "Smith",
                Balance = 1000.50m,
                OverdraftLimit = 700.00m
            }
        };
        // Act
        var accountsRepo = new Mock<IAccountsRepository>();
        accountsRepo.Setup(r => r.GetAccountsAsync())
            .ReturnsAsync(accounts);


        var accountsService = new AccountsService(accountsRepo.Object);
        var result = await accountsService.GetAccountsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNullOrEmpty();
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(accounts);
        result.Value.First().Should().BeEquivalentTo(accounts.First());
    }
}