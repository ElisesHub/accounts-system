namespace PortfolioFe.Infrastructure.ExternalClients.Configuration;

public class ExternalApiOptions
{
    public const string SectionName = "ExternalAccountsApi";

    public required string BaseUrl { get; init; }
}