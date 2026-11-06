using System.ComponentModel.DataAnnotations;

namespace OrchestrationApi.Infrastructure.ExternalClients.Configuration;

public class ExternalApiOptions : IValidatableObject
{
    public const string SectionName = "ExternalDbApi";

    [Required(ErrorMessage = "External API base URL is missing.")]
    [Url(ErrorMessage = "ExternalApiOptions:BaseUrl must be a valid absolute URI.")]
    public required string BaseUrl { get; init; } = string.Empty;

    public string? CaCertificatePath { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var errors = new List<ValidationResult>();

        if (!string.IsNullOrWhiteSpace(CaCertificatePath) && !File.Exists(CaCertificatePath))
            errors.Add(new ValidationResult("CA certificate not found ..."));

        return errors;
    }
}