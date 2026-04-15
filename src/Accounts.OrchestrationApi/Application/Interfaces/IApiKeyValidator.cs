namespace PortfolioApplicationAPI.Application.Interfaces;

public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}