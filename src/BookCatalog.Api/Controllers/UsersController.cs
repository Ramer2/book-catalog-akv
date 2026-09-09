using BookCatalog.Application.Requests.Loan.Query;
using BookCatalog.Application.Requests.User.Command;
using BookCatalog.Application.Requests.User.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetUserByIdQuery { Id = id }, cancellationToken));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUserById(
        [FromRoute] Guid id,
        [FromBody] UpdateUserByIdCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUserById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserByIdCommand { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpGet("{userId:guid}/loans")]
    public async Task<IActionResult> GetUserLoans(
        [FromRoute] Guid userId,
        [FromQuery] GetUserLoansQuery query,
        CancellationToken cancellationToken)
    {
        query.UserId = userId;
        return Ok(await _mediator.Send(query, cancellationToken));
    }
}