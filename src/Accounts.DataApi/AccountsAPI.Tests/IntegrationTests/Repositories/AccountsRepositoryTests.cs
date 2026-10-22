using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Tests.IntegrationTests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AccountsAPI.IntegrationTests;

public class
    AccountsRepositoryTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AccountsRepositoryTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAccountAsync_returns_single_account()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IAccountsRepository>();

        // Act
        var result = await repository.GetAccountAsync("1");

        // Assert
        result.Should().NotBeNull();

        result.Should().BeEquivalentTo( new
        {
            Id = AccountId.Create("1").Value,
            FirstName = "John",
            LastName = "Smith",
            Balance = 1500.50m,
            OverdraftLimit = 500.00m
        });
    }

    [Fact]
    public async Task GetAccountsAsync_returns_accounts()
    {
        //Arrange
        using var scope = _factory.Services.CreateScope();
        var respository = scope.ServiceProvider
            .GetRequiredService<IAccountsRepository>();
        //Act
        var result = await respository.GetAccountsAsync();

        //Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterThan(1);
        result.First().Should().BeEquivalentTo(
            new
            {
                Id = AccountId.Create("1").Value,
                FirstName = "John",
                LastName = "Smith",
                Balance = 1500.50m,
                OverdraftLimit = 500.00m
            });
    }
}