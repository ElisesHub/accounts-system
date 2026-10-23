using System;
using System.Collections.Generic;
using AccountsAPI.Infrastructure.Security;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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

        var options = Options.Create(new ApiKeyOptions {AccountsApiKey = storedKey});

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
        var options = Options.Create(new ApiKeyOptions { AccountsApiKey = storedKey });
        var validator = new ApiKeyValidator(options);

        // Act and Assert
        var exception = Assert.Throws<InvalidOperationException>(() => validator.IsValid(incomingKey));
        Assert.Equal("AccountsApiKey is not configured.", exception.Message);
    }


}