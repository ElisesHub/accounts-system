using System;
using AccountsAPI.Application.Models;
using AccountsAPI.Presentation.Helpers;
using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Microsoft.Extensions.Logging.Testing;

namespace AccountsAPI.Tests.UnitTests.Presentation.Helpers;

public class ResultTranslationHelperTests
{
    // A logger that discards everything, for tests that don't check logging
    private static readonly ILogger Logger = NullLogger.Instance;

    [Fact]
    public void GetValueOrThrow_returns_value_on_success()
    {
        // Arrange
        var result = Result<string>.Success("account data");

        // Act
        var value = result.GetValueOrThrow(Logger, "Account", "1");

        // Assert
        value.Should().Be("account data");
    }

    [Fact]
    public void GetValueOrThrow_throws_NotFound_with_the_result_message()
    {
        // Arrange
        var result = Result<string>.NotFound("Account 9999 was not found.");

        // Act
        Action act = () => result.GetValueOrThrow(Logger, "Account", "9999");

        // Assert
        act.Should().Throw<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound
                     && e.Status.Detail == "Account 9999 was not found.");
    }

    [Fact]
    public void GetValueOrThrow_uses_fallback_message_when_NotFound_has_none()
    {
        // Arrange
        var result = Result<string>.NotFound(null!);

        // Act
        Action act = () => result.GetValueOrThrow(Logger, "Account", "9999");

        // Assert
        act.Should().Throw<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound
                     && e.Status.Detail == "Account not found.");
    }

    [Fact]
    public void GetValueOrThrow_throws_InvalidArgument_with_the_validation_message()
    {
        // Arrange
        var result = Result<string>.Validation("accountId", "Account ID must be positive.");

        // Act
        Action act = () => result.GetValueOrThrow(Logger, "Account", "0");

        // Assert: Contains rather than equality, so this doesn't depend on the
        // formatter's exact layout, only on the message reaching the caller
        act.Should().Throw<RpcException>()
            .Where(e => e.StatusCode == StatusCode.InvalidArgument
                     && e.Status.Detail.Contains("Account ID must be positive."));
    }

    [Fact]
    public void GetValueOrThrow_throws_Internal_and_logs_when_success_has_no_value()
    {
        // Arrange
        var logger = new FakeLogger();
        var result = Result<string>.Success(null!);

        // Act
        Action act = () => result.GetValueOrThrow(logger, "Account", "1");

        // Assert: the caller gets a generic message, never the internal details...
        act.Should().Throw<RpcException>()
            .Where(e => e.StatusCode == StatusCode.Internal
                     && e.Status.Detail.StartsWith("An internal error occurred."));

        // ...and the details go to the log instead
        logger.LatestRecord.Level.Should().Be(LogLevel.Error);
        logger.LatestRecord.Message.Should().Contain("Account").And.Contain("1");
    }
}