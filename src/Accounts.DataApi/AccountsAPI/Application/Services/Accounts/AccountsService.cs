using AccountsAPI.Application.Dtos.Accounts;
using AccountsAPI.Application.Interfaces.Repositories;
using AccountsAPI.Application.Interfaces.Services;
using AccountsAPI.Application.Mappings;
using AccountsAPI.Application.Models;
using AccountsAPI.Domain.Entities;

namespace AccountsAPI.Application.Services.Accounts;

/// <summary>
/// Provides services for managing and retrieving account information.
/// </summary>
public class AccountsService(IAccountsRepository accountsRepository)
    : IAccountsService
{
    /// <summary>
    /// Retrieves a single account by its identifier asynchronously.
    /// </summary>
    /// <param name="accountId">The account ID supplied by the caller. Must be positive.</param>
    /// <param name="ct">A token to cancel the operation, for example when the caller's deadline passes.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> that is successful and contains the <see cref="AccountDto"/>,
    /// a validation failure if <paramref name="accountId"/> is not positive,
    /// or not found if no account has that ID.
    /// </returns>
    public async Task<Result<AccountDto>> GetAccountAsync(int accountId,
        CancellationToken ct)
    {
        var idResult = AccountId.Create(accountId);
        if (idResult.CurrentState != ResultStatus.Success)
        {
            return
                Result<AccountDto>.Validation(idResult.Errors);
        }

        var account = await accountsRepository.GetAccountAsync(idResult.Value, ct);

        if (account is null)
        {
            return Result<AccountDto>.NotFound("Account not found");
        }

        return Result<AccountDto>.Success(AccountMappings.ToAccountDto(account));
    }

    /// <summary>
    /// Retrieves all accounts asynchronously.
    /// </summary>
    /// <param name="ct">A token to cancel the operation, for example when the caller's deadline passes.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the accounts as <see cref="AccountDto"/>s.
    /// The list is empty if there are no accounts; it is never null.
    /// </returns>
    public async Task<Result<IReadOnlyList<AccountDto>>> GetAccountsAsync(
            CancellationToken ct)
    {
        var accounts = await accountsRepository.GetAccountsAsync(ct);

        return Result<IReadOnlyList<AccountDto>>.Success(AccountMappings.ToAccountDtoList(accounts));
    }
}