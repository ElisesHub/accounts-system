using PortfolioApplicationAPI.Presentation.Models;
 namespace PortfolioApplicationAPI.Presentation.ExceptionHandling;



using Microsoft.AspNetCore.Diagnostics;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}",
            httpContext.TraceIdentifier,
            httpContext.Request.Path);

        var statusCode = StatusCodes.Status500InternalServerError;
        var code = "SERVER_ERROR";
        var message = "An unexpected error occurred.";

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                code = "UNAUTHORIZED";
                message = "Unauthorized.";
                break;

            case HttpRequestException:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                code = "UPSTREAM_SERVICE_UNAVAILABLE";
                message = "A required upstream service could not be reached.";
                break;

            case TaskCanceledException:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                code = "REQUEST_TIMEOUT";
                message = "The request timed out.";
                break;

            case NotImplementedException:
                statusCode = StatusCodes.Status501NotImplemented;
                code = "NOT_IMPLEMENTED";
                message = "This functionality is not available.";
                break;
        }

        httpContext.Response.StatusCode = statusCode;

        var response = new ApiErrorResponse
        {
            Code = code,
            Message = message,
            TraceId = httpContext.TraceIdentifier,

            // only include developer detail outside production
            DeveloperMessage = environment.IsDevelopment() ? exception.Message : null
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}