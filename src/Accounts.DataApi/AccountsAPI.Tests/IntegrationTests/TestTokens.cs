using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AccountsAPI.Tests.IntegrationTests;

public static class TestTokens
{
    public const string Issuer = "https://test-issuer/";
    public const string Audience = "https://accounts-data-api";
    public static readonly SymmetricSecurityKey SigningKey = new(Encoding.UTF8.GetBytes("integration-signing-key-32-bytes-minimum-length"));

    public static string Create(
        string? scope = "read:accounts",
        string audience = Audience,
        DateTime? expires = null,
        SecurityKey? signingKey = null)
    {
        var expiry = expires ?? DateTime.UtcNow.AddMinutes(5);
        var claims = new Dictionary<string, object>();

        if (scope is not null)
            claims["scope"] = scope;

        var descriptor = new SecurityTokenDescriptor()
        {
            Issuer = Issuer,
            Audience = audience,
            Expires = expiry,
            NotBefore = expiry.AddMinutes(-30),
            IssuedAt = expiry.AddMinutes(-30),
            Claims = claims,
            SigningCredentials = new SigningCredentials(signingKey ?? SigningKey, SecurityAlgorithms.HmacSha256)

        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}