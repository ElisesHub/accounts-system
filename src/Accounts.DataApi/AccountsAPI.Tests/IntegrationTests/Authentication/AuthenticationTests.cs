using System;
using System.Threading.Tasks;
using Accounts.Api.V1;
using AwesomeAssertions;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace AccountsAPI.Tests.IntegrationTests.Authentication;

/// <summary>
/// Verifies that the Data API rejects calls without a valid token, or without the
/// read:accounts scope, before any service code runs.
/// </summary>
public class AuthenticationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private const int ExistingAccountId = 1;
    private static readonly SymmetricSecurityKey UntrustedKey =
        new("a-completely-different-key-of-32-bytes!!"u8.ToArray());
    private AccountsService.AccountsServiceClient CreateClient() =>
        new(factory.CreateGrpcChannel());

    [Fact]
    public async Task GetAccount_NoToken_ReturnsUnauthenticated()
    {
        // Arrange
        var client = CreateClient();

        // Act: no headers at all
        Func<Task> act = async () => await client.GetAccountAsync(
            new GetAccountRequest { AccountId = ExistingAccountId },
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var thrown = await act.Should().ThrowAsync<RpcException>();
        thrown.Which.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }

    /// <summary>
    /// Tokens the Data API must reject with Unauthenticated.
    /// Each row: a case name (shown in the test runner) and the token to send.
    /// </summary>
    public static TheoryData<string, string> InvalidTokens()
    {
        var cases = new TheoryData<string, string>
        {
            // --- Wrong signature ---
            // Correct claims, but signed with a key the server doesn't trust.
            { "wrong signing key", TestTokens.Create(signingKey: UntrustedKey) },
            // --- Wrong audience ---
            // Genuinely signed, but issued for a different API.
            { "wrong audience", TestTokens.Create(audience: "https://some-other-api") },
            // --- Expired ---
            // Past its expiry; 10 minutes is beyond the 5-minute clock skew the server allows.
            { "expired", TestTokens.Create(expires: DateTime.UtcNow.AddMinutes(-10)) },
            // --- Malformed ---
            // Not a JWT at all; should be a clean Unauthenticated, not an Internal error.
            { "not a JWT", "not-a-jwt" }
        };

        return cases;
    }

    [Theory]
    [MemberData(nameof(InvalidTokens))]
    public async Task GetAccount_InvalidToken_ReturnsUnauthenticated(string caseName, string token)
    {
        // Arrange
        _ = caseName; // shows which case failed in results but isn't needed in the test
        var client = CreateClient();

        // Act
        Func<Task> act = async () => await client.GetAccountAsync(
            new GetAccountRequest { AccountId = ExistingAccountId },
            headers: factory.Bearer(token),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var thrown = await act.Should().ThrowAsync<RpcException>();
        thrown.Which.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task GetAccount_ValidTokenWithoutScope_ReturnsPermissionDenied()
    {
        // Arrange: a genuine, correctly signed token, but without read:accounts
        var client = CreateClient();
        var token = TestTokens.Create(scope: null);

        // Act
        Func<Task> act = async () => await client.GetAccountAsync(
            new GetAccountRequest { AccountId = ExistingAccountId },
            headers: factory.Bearer(token),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert: authenticated, but not authorised
        var thrown = await act.Should().ThrowAsync<RpcException>();
        thrown.Which.StatusCode.Should().Be(StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task GetAccount_ValidTokenWithScope_Succeeds()
    {
        // Arrange
        var client = CreateClient();

        // Act: Bearer() with no argument = valid token with read:accounts
        var response = await client.GetAccountAsync(
            new GetAccountRequest { AccountId = ExistingAccountId },
            headers: factory.Bearer(),
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Account.Should().NotBeNull();
        response.Account.AccountId.Should().Be(ExistingAccountId);
    }
}