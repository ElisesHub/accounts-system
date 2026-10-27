using AccountsAPI.Application.Dtos.Accounts;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Mappings;
using AccountsAPI.Application.Models;
using AccountsAPI.Domain.Entities;
using
    AccountsAPI.Infrastructure.Repositories; // ⚠ Still here: Application → Infrastructure. Delete it.
//   IAccountsRepository comes from Application.Interfaces.

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
    // ⚠ Stale: no incomingApiKey parameter. Add a <param> for ct instead.
    /// <returns>The <see cref="Account"/> object if found, or null if no account matches the specified identifier.</returns>
    // ⚠ Stale: it returns a Result<AccountDto>: Success, NotFound or ValidationError. Never null.
    /// <exception cref="Exception">Thrown when no account is found with the specified identifier.</exception>
    // ⚠ Stale: not-found doesn't throw. Delete this line.
    public async Task<Result<AccountDto>> GetAccountAsync(int accountId,
        CancellationToken ct)
    {
        var idResult = AccountId.Create(accountId);
        if (idResult.CurrentState != ResultStatus.Success)
        {
            return
                Result<AccountDto>.Validation(idResult.Errors);
        }

        var account = await accountsRepository.GetAccountAsync(accountId, ct);

        if (account is null)
        {
            return Result<AccountDto>.NotFound("Account not found");
        }

        return Result<AccountDto>.Success(AccountMappings.ToAccountDto(account));
        // ToResponse → ToDto, to match the renamed type.
    }

    /// <summary>
    /// Retrieves all accounts asynchronously.
    /// </summary>
    /// <param name="ct">A token to cancel the operation, for example when the caller's deadline passes.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the accounts as <see cref="AccountDto"/>s.
    /// The list is empty if there are no accounts; it is never null.
    /// </returns>
    public async Task<Result<IReadOnlyList<AccountDto>?>> GetAccountsAsync(
            CancellationToken ct)
        // Drop the `?` (here and in the interface): a successful result always has a list.
    {
        var accounts = await accountsRepository.GetAccountsAsync(ct);

        if (accounts == null || accounts.Count == 0)
        {
            return Result<IReadOnlyList<AccountDto>?>.NotFound(
                "No accounts found");
        }
        // ⚠ Design question: is an empty list "not found"?
        //   NotFound means "the thing you asked for doesn't exist". But the thing asked for is
        //   *the collection*, and it exists: it just has no items. The usual convention (REST
        //   200 with [], gRPC OK with an empty repeated field) is Success with an empty list.
        //   Otherwise every caller has to treat NotFound as "maybe zero accounts", which is an
        //   implicit convention again.
        //   `accounts == null` also can't happen any more, since the repository returns a list.

        return Result<IReadOnlyList<AccountDto>?>.Success(
            AccountMappings.ToAccountDtoList(accounts));
        // ToResponseList → ToDtoList, or ToDtos.
    }
}