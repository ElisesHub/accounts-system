using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Services;
using AccountsAPI.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AccountsAPI.Tests.UnitTests.Services;

public class AccountsServiceTests
{
    [Fact]
    public async Task GetAccount_returns_single_account()
    {
        // Arrange
        var returnedAccount = new Account()
        {
            Id=1,
            FirstName = "John",
            LastName = "Smith",
            Balance = 1500.50m,
            OverdraftLimit = 500.00m
        };

        var accountsRepo = new Mock<IAccountsRepository>();
        accountsRepo
            .Setup(r => r.GetAccountAsync("1"))
            .ReturnsAsync(returnedAccount);

        var apiKeyValidator = new Mock<IApiKeyValidator>();
        apiKeyValidator.Setup(r => r.IsValid(It.IsAny<string>())).Returns(true);

        var service = new AccountsService(accountsRepo.Object, apiKeyValidator.Object);

        // Act
        var result = await service.GetAccountAsync("1", "test");

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(returnedAccount);

    }
    [Fact]
    public async Task GetAccounts_returns_list_of_accounts()
    {
        // Arrange
        var accounts = new List<Account>()
        {
            new()
            {
                Id=1,
                FirstName = "John",
                LastName = "Smith",
                Balance = 1500.50m,
                OverdraftLimit = 500.00m
            },
            new()
            {
                Id=1,
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

        var apiKeyValidator = new Mock<IApiKeyValidator>();
        apiKeyValidator.Setup(r => r.IsValid(It.IsAny<string>())).Returns(true);

        var accountsService = new AccountsService(accountsRepo.Object, apiKeyValidator.Object);
        var result = await accountsService.GetAccountsAsync("test");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(accounts);
        result.First().Should().BeEquivalentTo(accounts.First());


    }
}