using System.ComponentModel.DataAnnotations;

namespace OrchestrationApi.Infrastructure.ExternalClients.Auth0;

internal class Auth0Options
{
    public const string SectionName = "Auth0";
    [Required(ErrorMessage = "Auth0 Client ID is required for the app to start")]
    public string ClientId { get; init; } = string.Empty;

    [Required(ErrorMessage = "Auth0 ClientSecret is required for the app to start")]
    public string ClientSecret { get; init; } = string.Empty;

    [Required(ErrorMessage = "Auth0 Audience is required for the app to start")]
    public string Audience { get; init; } = string.Empty;

    [Required(ErrorMessage = "Auth0 Domain is required for the app to start")]
    public string Domain { get; init; } = string.Empty;
}