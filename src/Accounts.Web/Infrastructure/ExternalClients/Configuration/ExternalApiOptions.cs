using System.ComponentModel.DataAnnotations;

namespace PortfolioFe.Infrastructure.ExternalClients.Configuration;

public class ExternalApiOptions
{
    public const string SectionName = "ExternalAccountsApi";

    [Required(ErrorMessage = "External API base URL is missing.")]
    [Url(ErrorMessage = "ExternalApiOptions:BaseUrl must be a valid absolute URI.")]
    public required string BaseUrl { get; init; }



}