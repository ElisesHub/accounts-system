using System;
using System.Collections.Generic;
using AccountsAPI.Infrastructure.Security;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AccountsAPI.Tests.Infrastructure.Security;

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
    [InlineData("sk_live_abc123", "sk_test_abc123", false)] // wrong prefix
    public void isValid_returns_expected_result(string incomingKey, string storedKey, bool expectedResult)
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["AccountsAPIKey"] = storedKey
        };

        var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(configData)
        .Build();

        var validator = new ApiKeyValidator(configuration);

        // Act
        var result = validator.IsValid(incomingKey);

        // Assert
        Assert.True(expectedResult == result);
    }


    [Theory]
    [InlineData("apikey123", "")]             // empty stored key
    [InlineData("apikey123", null)]           // null stored key
    [InlineData("apikey123", "   ")]         // whitespace only stored key
    public void isValid_returns_expected_throws_exception(string incomingKey, string storedKey)
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            ["AccountsAPIKey"] = storedKey
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var validator = new ApiKeyValidator(configuration);

        // Act and Assert
        var exception = Assert.Throws<Exception>(()=> validator.IsValid(incomingKey));
        Assert.Equal("Key is not set in configuration.", exception.Message);
    }


}