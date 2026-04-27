using AccountsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Presentation.Controllers;

namespace AccountsApplicationAPI.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IAccountsService accountsService, IConfiguration configuration)
        : BaseApiController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {

                var result =
                    await accountsService.GetAccountAsync(id.ToString());

                return FromResult(result);

        }
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
                var result = await accountsService.GetAccountsAsync();

                return FromResult(result);

        }



    }