using Microsoft.AspNetCore.Authentication;

namespace PortfolioApplicationAPI.Presentation.Models.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "ApiKey";
    public string HeaderName { get; set; } = "x-api-key";
}