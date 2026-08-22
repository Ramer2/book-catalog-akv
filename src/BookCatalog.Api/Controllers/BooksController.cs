using BookCatalog.Application.Requests.Book.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllBooks([FromQuery] GetAllBooksQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query));
    }
}