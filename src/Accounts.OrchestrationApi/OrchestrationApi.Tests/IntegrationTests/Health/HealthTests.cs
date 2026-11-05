using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using OrchestrationApi.Tests.Infrastructure;
using Xunit;

namespace OrchestrationApi.Tests.IntegrationTests.Health;

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


}