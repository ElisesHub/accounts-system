namespace PortfolioApplicationAPI.Infrastructure.ExternalClients.Configuration;

public class ExternalApiOptions
{
    public const string SectionName = "ExternalDbApi";
    public required string BaseUrl { get; init; } = string.Empty;
}