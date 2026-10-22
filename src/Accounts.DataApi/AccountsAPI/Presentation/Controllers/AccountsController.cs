using AccountsAPI.Application.Interfaces;
using AccountsAPI.Application.Services;
using AccountsAPI.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountsAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireApiKey")]
public class AccountsController(
    IAccountsService accountsService,
    IConfiguration configuration)
    : BaseApiController
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
            var result =
                await accountsService.GetAccountAsync(id.ToString());

            return FromResult(result);
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
            var result = await accountsService.GetAccountsAsync();

            return FromResult(result);
    }



}