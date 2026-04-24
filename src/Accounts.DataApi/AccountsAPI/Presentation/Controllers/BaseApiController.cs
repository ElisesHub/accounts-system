using AccountsAPI.Application.Models;
using AccountsAPI.Presentation.Models;
using Microsoft.AspNetCore.Mvc;

namespace AccountsAPI.Presentation.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {

        if (result.IsValidationError)
        {
            return BadRequest(new ApiErrorResponse
            {
                Code = ApiErrorCodes.ValidationError.ToString(),
                Message = result.Message ?? "One or more validation errors occurred.",
                FieldErrors = result.Errors,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        if (result.IsNotFound)
        {
            return NotFound(new ApiErrorResponse
            {
                Code = ApiErrorCodes.NotFound.ToString(),
                Message = result.Message ?? "Resource not found.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(result.Value);
    }
}