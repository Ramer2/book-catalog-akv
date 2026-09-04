using AutoMapper;
using BookCatalog.Application.Requests.Loan.Query;
using BookCatalog.Application.Responses.Loan;
using BookCatalog.Application.Services.Loan;
using MediatR;

namespace BookCatalog.Application.Handlers.Loan.Query;

public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanResponse>
{
    private readonly ILoanService _loanService;
    private readonly IMapper _mapper;

    public GetLoanByIdQueryHandler(IMapper mapper, ILoanService loanService)
    {
        _mapper = mapper;
        _loanService = loanService;
    }

    public async Task<LoanResponse> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
    {
        var loan = await _loanService.GetOrThrowAsync(request.Id, cancellationToken);
        return _mapper.Map<LoanResponse>(loan);
    }
}
