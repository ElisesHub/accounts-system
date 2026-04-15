
using AccountsApplicationAPI.Application.Interfaces;
using AccountsApplicationAPI.Models;
using PortfolioApplicationAPI.Application.Interfaces;

namespace AccountsAPI.Services;

public class AccountsService(IExternalAccountsClient externalAccountsClient, IApiKeyValidator apiKeyValidator ) : IAccountsService
{
    public async Task<Account?> GetAccountAsync(string accountId, string incomingApiKey)
    {
        ValidateIncomingApiKey(incomingApiKey);
        ValidateAccountId(accountId);
        var account = await externalAccountsClient.GetAccountAsync(accountId);
        if(account == null) throw new Exception("Account not found");
        return account;
    }

    public async Task<IEnumerable<Account>?> GetAccountsAsync(string incomingApiKey)
    {
        ValidateIncomingApiKey(incomingApiKey);
        var accounts = await externalAccountsClient.GetAccountsAsync();
        if(accounts == null) throw new Exception("No accounts found");
        return accounts;
    }

    private void ValidateIncomingApiKey(string incomingApiKey)
    {
        if (!apiKeyValidator.IsValid(incomingApiKey)) throw new UnauthorizedAccessException("Invalid API key");
    }

    private void ValidateAccountId(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId)) throw new ArgumentException("Account id is required", nameof(accountId));
        if (!int.TryParse(accountId, out _)) throw new ArgumentException("Account id must be a number", nameof(accountId));

        if (accountId == "0") throw new ArgumentException("Account id cannot be 0", nameof(accountId));
    }
}