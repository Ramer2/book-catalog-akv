using AutoMapper;
using BookCatalog.Application.Requests.Loan.Command;
using BookCatalog.Application.Responses.Loan;
using BookCatalog.Application.Services.Loan;
using MediatR;

namespace BookCatalog.Application.Handlers.Loan.Command;

public class ReturnLoanCommandHandler : IRequestHandler<ReturnLoanCommand, LoanResponse>
{
    private readonly ILoanService _loanService;
    private readonly IMapper _mapper;

    public ReturnLoanCommandHandler(IMapper mapper, ILoanService loanService)
    {
        _mapper = mapper;
        _loanService = loanService;
    }

    public async Task<LoanResponse> Handle(ReturnLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanService.ReturnAsync(request.Id, cancellationToken);
        return _mapper.Map<LoanResponse>(loan);
    }
}
