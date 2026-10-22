namespace AccountsAPI.Application.Interfaces;

public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}