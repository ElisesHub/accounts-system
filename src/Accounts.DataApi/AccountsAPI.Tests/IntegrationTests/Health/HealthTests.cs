using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using AwesomeAssertions;

namespace AccountsAPI.Tests.IntegrationTests.Health;

public class HealthTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Live_ReturnsOk_WithoutAuthentication()
    {
        //Arrange
        using var client = factory.CreateClient();
        //Act
        var result = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_ReturnsOk_WithoutAuthentication()
    {
        //Arrange
        using var client = factory.CreateClient();
        //Act
        var result = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}