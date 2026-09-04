using BookCatalog.Application.Requests.Book.Command;
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
    public async Task<IActionResult> GetAllBooks([FromQuery] GetAllBooksQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAllBooks([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetBookByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet("isbn/{isbn}")]
    public async Task<IActionResult> GetBookByIsbn([FromRoute] string isbn, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetBookByIsbnQuery { Isbn = isbn }, cancellationToken));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBookById([FromRoute] Guid id, [FromBody] UpdateBookByIdCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBookById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBookByIdCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}