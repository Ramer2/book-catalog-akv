using AutoMapper;
using BookCatalog.Application.Requests.Loan.Command;
using BookCatalog.Application.Responses.Loan;
using BookCatalog.Application.Services.Loan;
using MediatR;

namespace BookCatalog.Application.Handlers.Loan.Command;

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, LoanResponse>
{
    private readonly ILoanService _loanService;
    private readonly IMapper _mapper;

    public BorrowBookCommandHandler(IMapper mapper, ILoanService loanService)
    {
        _mapper = mapper;
        _loanService = loanService;
    }

    public async Task<LoanResponse> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanService.BorrowAsync(request.BookId, request.UserId, cancellationToken);
        return _mapper.Map<LoanResponse>(loan);
    }
}
