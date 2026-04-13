using Microsoft.AspNetCore.Mvc;
using PortfolioFe.Services;

namespace PortfolioFe.Controllers.Bank;

public class AccountsController(AccountsService accountsService) : Controller
{
    public async Task<IActionResult> GetAccount(int id)
    {
        var account = await accountsService.GetAccountAsync(id.ToString());
        return View("~/Views/Bank/SingleAccountPartial.cshtml", account);
    }

    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await accountsService.GetAccountsAsync();
        return View("~/Views/Bank/AllAccountsPartial.cshtml", accounts);
    }
}