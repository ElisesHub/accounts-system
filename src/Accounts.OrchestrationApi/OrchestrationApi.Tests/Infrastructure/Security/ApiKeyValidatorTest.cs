using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using OrchestrationApi.Infrastructure.Security;
using OrchestrationApi.Infrastructure.Security.ApiKeys;
using Xunit;
using AwesomeAssertions;
namespace OrchestrationApi.Tests.Infrastructure.Security;

[TestSubject(typeof(ApiKeyValidator))]
public class ApiKeyValidatorTest
{

    [Theory]
    [InlineData("apikey123", "apikey123", true)]            // exact match
    [InlineData("short", "short", true)]                    // short but matching
    [InlineData("sk_live_abc123", "sk_live_abc123", true)]  // realistic key
    [InlineData("apikey123", "apikey124", false)]           // mismatch
    [InlineData("APIKEY123", "apikey123", false)]           // case-sensitive mismatch
    [InlineData("key-with-dash", "key-with-dash ", false)]  // trailing space
    [InlineData(null, "apikey123", false)]                  // null input
    [InlineData("sk_live_abc123", "sk_test_abc123", false)] // differs in midddle
    [InlineData("", "apikey123", false)] // empty incoming key
    [InlineData("   ", "apikey123", false)] // empty (whitespace) incoming key
    public void isValid_returns_expected_result(string incomingKey, string storedKey, bool expectedResult)
    {

        var options = Options.Create(new ApiKeyOptions {AccountsApplicationApiKey = storedKey});

        var validator = new ApiKeyValidator(options);

        // Act
        var result = validator.IsValid(incomingKey);

        // Assert
        Assert.Equal(expectedResult, result);
    }


    [Theory]
    [InlineData("apikey123", "")]       // empty stored key
    [InlineData("apikey123", null)]     // null stored key
    [InlineData("apikey123", "   ")]    // whitespace-only stored key
    public void IsValid_throws_when_stored_key_is_not_configured(string incomingKey, string? storedKey)
    {
        // Arrange
        var options = Options.Create(new ApiKeyOptions { AccountsApplicationApiKey = storedKey! });
        var validator = new ApiKeyValidator(options);
        Action act = () => validator.IsValid(incomingKey);
        // Act and Assert
        var result =  act.Should().Throw<InvalidOperationException>();
        result.Which.Message.Should().Contain("Api key not found");
    }
}