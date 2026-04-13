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
            if (!IsAuthorised())
            {
                return Unauthorized("Unable to access resource.");
            }

            var account =
                await accountsService.GetAccountAsync(id.ToString());

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
            if (!IsAuthorised())
            {
                return Unauthorized("Unable to access resource.");
            }

            var accounts = await accountsService.GetAccountsAsync();

            return Ok(accounts ?? []);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Checks if the incoming request is authorized by validating the provided API key
    /// against the stored key in the configuration.
    /// </summary>
    /// <returns>
    /// True if the incoming API key matches the stored API key in the configuration;
    /// otherwise, false.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when the stored API key is not set in the configuration.
    /// </exception>
    private bool IsAuthorised()
    {
        if (!Request.Headers.TryGetValue("x-api-key", out var incomingKey))
        {
            return false;
        }

        var storedKey = configuration.GetValue<string>("AccountsAPIKey");
        if (string.IsNullOrWhiteSpace(storedKey))
        {
            throw new Exception("Key is not set in configuration.");
        }

        return string.Equals(incomingKey, storedKey, StringComparison.Ordinal);
    }
}