using System.ComponentModel.DataAnnotations;

namespace PortfolioApplicationAPI.Infrastructure.Security.ApiKeys;

public class ApiKeyOptions
{

    [Required(ErrorMessage = "Accounts Application API Key is required at startup")]public string AccountsApplicationApiKey { get; init; } =  string.Empty;
}