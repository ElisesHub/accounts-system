using Microsoft.AspNetCore.Mvc;
using PortfolioApplicationAPI.Application.Interfaces;

namespace PortfolioApplicationAPI.Presentation.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IAccountsService accountsService)
        : BaseApiController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id, CancellationToken ct)
        {

                var result =
                    await accountsService.GetAccountAsync(id, ct);

                return FromResult(result);

        }
        [HttpGet]
        public async Task<IActionResult> GetAccounts(CancellationToken ct)
        {
                var result = await accountsService.GetAccountsAsync(ct);

                return FromResult(result);

        }



    }