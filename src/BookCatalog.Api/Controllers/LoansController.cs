using BookCatalog.Application.Requests.Loan.Command;
using BookCatalog.Application.Requests.Loan.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllLoans([FromQuery] GetAllLoansQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLoanById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetLoanByIdQuery { Id = id }, cancellationToken));
    }

    [HttpPost("")]
    public async Task<IActionResult> BorrowBook([FromBody] BorrowBookCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPatch("{id:guid}/return")]
    public async Task<IActionResult> ReturnLoan([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new ReturnLoanCommand { Id = id }, cancellationToken));
    }
}