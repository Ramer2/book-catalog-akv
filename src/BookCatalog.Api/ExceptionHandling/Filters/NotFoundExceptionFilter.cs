using BookCatalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookCatalog.Api.ExceptionHandling.Filters;

public sealed class NotFoundExceptionFilter : IExceptionFilter
{
    private readonly ILogger<NotFoundExceptionFilter> _logger;

    public NotFoundExceptionFilter(ILogger<NotFoundExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not EntityNotFoundException ex)
        {
            return;
        }

        _logger.LogWarning(
            ex,
            "Not Found occurred while processing {Method} {Path}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path);

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = ex.Message
        })
        {
            StatusCode = StatusCodes.Status404NotFound
        };
        context.ExceptionHandled = true;
    }
}
