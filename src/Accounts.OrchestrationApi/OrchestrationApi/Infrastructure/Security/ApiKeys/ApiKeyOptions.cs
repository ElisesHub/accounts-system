using System.ComponentModel.DataAnnotations;

namespace OrchestrationApi.Infrastructure.Security.ApiKeys;

public class ApiKeyOptions
{

    [Required(ErrorMessage = "Accounts Application API Key is required at startup")]
    public string AccountsApplicationApiKey { get; init; } =  string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        throw new NotImplementedException();
    }
}