using Microsoft.Extensions.Options;
using OrchestrationApi.Application.Interfaces;
using OrchestrationApi.Infrastructure.Security.ApiKeys;

namespace OrchestrationApi.Infrastructure.Security;

public class ApiKeyValidator(IOptions<ApiKeyOptions> options) : IApiKeyValidator
{
    public bool IsValid(string incomingKey)
    {
        if (!CheckIsValidApiKey(incomingKey))
        {
            return false;
        }

        var storedKey = GetApiKey();
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

    private string GetApiKey()
    {
        if (options?.Value is null)
        {
            throw new InvalidOperationException("ApiKeyOptions not configured.");
        }
        var key = options?.Value?.AccountsApplicationApiKey;

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Api key not found.");
        }

        return key;

    }
}