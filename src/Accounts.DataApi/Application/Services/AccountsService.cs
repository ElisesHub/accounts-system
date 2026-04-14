using AccountsAPI.Application.Interfaces;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Infrastructure.Repositories;

namespace AccountsAPI.Application.Services;

/// <summary>
/// Provides services for managing and retrieving account information.
/// </summary>
public class AccountsService(IAccountsRepository accountsRepository, IApiKeyValidator apiKeyValidator)
    : IAccountsService
{
    /// <summary>
    /// Retrieves an account by its unique identifier asynchronously.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account to retrieve.</param>
    /// <param name="incomingApiKey"></param>
    /// <returns>The <see cref="Account"/> object if found, or null if no account matches the specified identifier.</returns>
    /// <exception cref="Exception">Thrown when no account is found with the specified identifier.</exception>
    public async Task<Account?> GetAccountAsync(string accountId, string incomingApiKey)
    {
        ValidateIncomingApiKey(incomingApiKey);
        ValidateAccountId(accountId);

        var account = await accountsRepository.GetAccountAsync(accountId);
        if(account == null) throw new Exception("Account not found");
        return account;
    }

    /// <summary>
    /// Retrieves a list of all accounts asynchronously.
    /// </summary>
    /// <returns>A list of <see cref="Account"/> objects, or null if no accounts are found.</returns>
    /// <exception cref="Exception">Thrown when no accounts are found in the repository.</exception>
    public async Task<List<Account>?> GetAccountsAsync(string incomingApiKey)
    {
        ValidateIncomingApiKey(incomingApiKey);

        var accounts = await accountsRepository.GetAccountsAsync();
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