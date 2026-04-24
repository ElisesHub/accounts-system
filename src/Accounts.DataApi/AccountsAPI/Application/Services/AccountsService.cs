using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Models;
using AccountsAPI.Domain.Entities;
using AccountsAPI.Infrastructure.Repositories;

namespace AccountsAPI.Application.Services;

/// <summary>
/// Provides services for managing and retrieving account information.
/// </summary>
public class AccountsService(IAccountsRepository accountsRepository)
    : IAccountsService
{
    /// <summary>
    /// Retrieves an account by its unique identifier asynchronously.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account to retrieve.</param>
    /// <param name="incomingApiKey"></param>
    /// <returns>The <see cref="Account"/> object if found, or null if no account matches the specified identifier.</returns>
    /// <exception cref="Exception">Thrown when no account is found with the specified identifier.</exception>
    public async Task<Result<Account>> GetAccountAsync(string accountId)
    {
        var result = AccountId.Create(accountId);
        if (!result.IsSuccess)
        {
            return Result<Account>.Validation(result.Errors);
        }
        var account = await accountsRepository.GetAccountAsync(accountId);

        if (account == null || account.Id == default)
        {
            return Result<Account>.NotFound("Account not found");
        }

        return Result<Account>.Success(account);
    }

    /// <summary>
    /// Retrieves a list of all accounts asynchronously.
    /// </summary>
    /// <returns>A list of <see cref="Account"/> objects, or null if no accounts are found.</returns>
    /// <exception cref="Exception">Thrown when no accounts are found in the repository.</exception>
    public async Task<Result<List<Account>?>> GetAccountsAsync()
    {
        var accounts = await accountsRepository.GetAccountsAsync();
        if (accounts == null || accounts.Count == 0)
        {
            return Result<List<Account>?>.NotFound("No accounts found");
        }

        return Result<List<Account>?>.Success(accounts);
    }



}