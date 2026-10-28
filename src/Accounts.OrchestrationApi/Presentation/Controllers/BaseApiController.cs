using FluentResults;
using Microsoft.AspNetCore.Mvc;
using PortfolioApplicationAPI.Application.Common.Errors;
using PortfolioApplicationAPI.Presentation.Models;

namespace PortfolioApplicationAPI.Presentation.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsFailed)
        {
            if(result.HasError<NotFoundError>())
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = ApiErrorCodes.NotFound.ToString(),
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Resource not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (result.HasError<ValidationError>())
            {
                return BadRequest(new ApiErrorResponse
                {
                Code = ApiErrorCodes.ValidationError.ToString(),
                Message = result.Errors.FirstOrDefault()?.Message ?? "Validation Error",
                TraceId = HttpContext.TraceIdentifier
                });
            }

            throw new InvalidOperationException(
                $"Unhandled result error type(s): {string.Join(", ", result.Errors.Select(e => e.GetType().Name).ToList())}");
        }

        return Ok(result.Value);
    }
}