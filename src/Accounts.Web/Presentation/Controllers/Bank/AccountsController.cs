using Microsoft.AspNetCore.Mvc;
using PortfolioFe.Application.Interfaces;
using PortfolioFe.Application.Services;

namespace PortfolioFe.Presentation.Controllers.Bank;

public class AccountsController(IAccountsService accountsService) : Controller
{
    [Route("/bank/account/{id:int}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        var account = await accountsService.GetAccountAsync(id.ToString());
        return View("~/Presentation/Views/Bank/SingleAccountPartial.cshtml", account);
    }
    [Route("/bank/accounts/")]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await accountsService.GetAccountsAsync();
        return View("~/Presentation/Views/Bank/AllAccountsPartial.cshtml", accounts);
    }
}