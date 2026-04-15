using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountsAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(
    IAccountsService accountsService,
    IConfiguration configuration)
    : ControllerBase
{
    /// <summary>
    /// Retrieves the details of a specific account by its identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the account to retrieve.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the account details if found,
    /// a 404 Not Found response if the account does not exist,
    /// a 400 Bad Request response if the identifier is invalid,
    /// or a 401 Unauthorized response if access is denied.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when an unexpected error occurs during the operation.
    /// </exception>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        try
        {
            var incomingApiKey = GetIncomingApiKey();
            var account =
                await accountsService.GetAccountAsync(id.ToString(), incomingApiKey);

            if (account == null) return NotFound();
            if (account.Id != id) return BadRequest("Invalid account id");

            return Ok(account);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An unexpected error occurred.");

        }
    }

    /// <summary>
    /// Retrieves a list of all accounts from the data source.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of accounts if available,
    /// or an empty list if no accounts are found.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when an unexpected error occurs during the operation.
    /// </exception>
    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        try
        {
            var incomingApiKey = GetIncomingApiKey();
            var accounts = await accountsService.GetAccountsAsync(incomingApiKey);

            return Ok(accounts ?? []);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Extracts the incoming API key from the request headers for authorization purposes.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> containing the API key if successfully retrieved from the request headers.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the API key is missing or invalid in the request headers.
    /// </exception>
    private string? GetIncomingApiKey()
    {
        if (!Request.Headers.TryGetValue("x-api-key", out var incomingKey))
        {
            throw new UnauthorizedAccessException("API key is not set.");
        }

        if (string.IsNullOrWhiteSpace(incomingKey))
        {
            throw new UnauthorizedAccessException("API key is not set.");
        }

        return incomingKey;
    }


}