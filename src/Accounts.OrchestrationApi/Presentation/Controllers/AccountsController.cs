using AccountsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountsApplicationAPI.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IAccountsService accountsService, IConfiguration configuration)
        : ControllerBase
    {
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
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {

            try
            {
                var incomingApiKey = GetIncomingApiKey(); ;

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