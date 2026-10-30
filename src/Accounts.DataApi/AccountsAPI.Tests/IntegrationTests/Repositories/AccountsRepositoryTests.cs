using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Interfaces.Repositories;
using AccountsAPI.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AccountsAPI.Tests.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for <see cref="IAccountsRepository"/> against a real MySQL database
/// started with Testcontainers and seeded from the SQL dump.
/// Docker must be running.
/// </summary>
public class
    AccountsRepositoryTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetAccountAsync_returns_single_account()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IAccountsRepository>();

        // Act
        var result = await repository.GetAccountAsync(AccountId.Create(1).Value, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.Should().BeEquivalentTo( new
        {
            Id = AccountId.Create(1).Value,
            FirstName = "John",
            LastName = "Smith",
            Balance = 280.00m,
            OverdraftLimit = 500.00m
        });
    }

    [Fact]
    public async Task GetAccountsAsync_returns_accounts()
    {
        //Arrange
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IAccountsRepository>();
        //Act
        var result = await repository.GetAccountsAsync(CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterThan(1);
        result.First().Should().BeEquivalentTo(
            new
            {
                Id = AccountId.Create(1).Value,
                FirstName = "John",
                LastName = "Smith",
                Balance = 280.00m,
                OverdraftLimit = 500.00m
            });
    }
}