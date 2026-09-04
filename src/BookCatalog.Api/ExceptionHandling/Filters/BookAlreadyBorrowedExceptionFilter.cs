using BookCatalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookCatalog.Api.ExceptionHandling.Filters;

public sealed class BookAlreadyBorrowedExceptionFilter : IExceptionFilter
{
    private readonly ILogger<BookAlreadyBorrowedExceptionFilter> _logger;

    public BookAlreadyBorrowedExceptionFilter(ILogger<BookAlreadyBorrowedExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not BookAlreadyBorrowedException ex)
        {
            return;
        }

        _logger.LogWarning(
            ex,
            "Conflict occurred while processing {Method} {Path}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path);

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = ex.Message
        })
        {
            StatusCode = StatusCodes.Status409Conflict
        };
        context.ExceptionHandled = true;
    }
}
