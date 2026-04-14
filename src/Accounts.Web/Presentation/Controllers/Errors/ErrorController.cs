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
                Response.StatusCode = 500;
                return View("~/Presentation/Views/Errors/Error.cshtml");

            default:
                Response.StatusCode = 500;
                return View("~/Presentation/Views/Errors/Error.cshtml");
        }

        ;
    }
}