using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Requests.Author.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllAuthors([FromQuery] GetAllAuthorsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAuthorById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetAuthorByIdQuery { Id = id }, cancellationToken));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAuthorById(
        [FromRoute] Guid id,
        [FromBody] UpdateAuthorByIdCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAuthorById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAuthorByIdCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
