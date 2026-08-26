using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookCatalog.Api.ExceptionHandling.Filters;

public sealed class UnhandledExceptionFilter : IExceptionFilter
{
    private readonly ILogger<UnhandledExceptionFilter> _logger;

    public UnhandledExceptionFilter(ILogger<UnhandledExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled)
        {
            return;
        }

        var exception = context.Exception;

        _logger.LogError(
            exception,
            "Unhandled exception occurred while processing {Method} {Path}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path);

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = exception.Message
        })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }
}
