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

        private bool IsAuthorised()
        {
            if (!Request.Headers.TryGetValue("x-api-key", out var incomingKey))
            {
                return false;
            }
            var storedKey = configuration.GetValue<string>("AppAPIKey");
            if (string.IsNullOrWhiteSpace(storedKey))
            {
                throw new Exception("Key is not set in configuration.");
            }

            return string.Equals(incomingKey, storedKey, StringComparison.Ordinal);
        }

    }