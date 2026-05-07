using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioFe.Presentation.Controllers.Errors;

public class ErrorController : Controller
{
    [Route("/Error")]
    public IActionResult Error()
    {
        var exceptionFeature =
            HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;


        switch (exception)
        {
            case KeyNotFoundException:
                Response.StatusCode = 404;
                return View("~/Presentation/Views/Errors/NotFound.cshtml");

            case UnauthorizedAccessException:
                Response.StatusCode = 403;
                return View("~/Presentation/Views/Errors/Unauthorized.cshtml");

            case HttpRequestException:
                ViewBag.StatusCode = 404;
                return View("~/Presentation/Views/Errors/Error.cshtml");

            default:
                ViewBag.StatusCode = 500;
                return View("~/Presentation/Views/Errors/Error.cshtml");
        }


    }
    [Route("Error/StatusCode")]
    public IActionResult StatusCode(int code)
    {
        if (code == 404)
        {
            return View("~/Presentation/Views/Errors/NotFound.cshtml");
        }

        ViewBag.StatusCode = code;
        return View("~/Presentation/Views/Errors/Error.cshtml");
    }

    [Route("Error/ServerError")]
    public IActionResult ServerError()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        // Optional: log exceptionFeature?.Error here

        return View("~/Presentation/Views/Errors/Error.cshtml");
    }
}