using PortfolioApplicationAPI.Application.Interfaces;

namespace PortfolioApplicationAPI.Infrastructure.Security;

public class ApiKeyValidator(IConfiguration configuration) : IApiKeyValidator
{
    public bool IsValid(string incomingKey)
    {
        if (!CheckIsValidApiKey(incomingKey))
        {
            return false;
        }

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
    private bool CheckIsValidApiKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return true;
    }
}