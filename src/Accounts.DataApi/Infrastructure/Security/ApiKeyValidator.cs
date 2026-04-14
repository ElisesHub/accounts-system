using AccountsAPI.Application.Interfaces;

namespace AccountsAPI.Infrastructure.Security;

public class ApiKeyValidator(IConfiguration configuration) : IApiKeyValidator
{
    public bool IsValid(string incomingKey)
    {
        var storedKey = configuration.GetValue<string>("AccountsAPIKey");
        if (string.IsNullOrWhiteSpace(storedKey))
        {
            throw new Exception("Key is not set in configuration.");
        }
        return IsMatch(incomingKey, storedKey);
    }
    private bool IsMatch(string incomingKey, string storedKey)
    {
        return string.Equals(incomingKey, storedKey, StringComparison.Ordinal);
    }
}