using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Loan.Query;
using BookCatalog.Application.Responses.Loan;
using MediatR;

namespace BookCatalog.Application.Handlers.Loan.Query;

public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, GetAllLoansResponse>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IMapper _mapper;

    public GetAllLoansQueryHandler(ILoanRepository loanRepository, IMapper mapper)
    {
        _loanRepository = loanRepository;
        _mapper = mapper;
    }

    public async Task<GetAllLoansResponse> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
    {
        var loans = await _loanRepository.GetAllAsync(request, cancellationToken);
        return new GetAllLoansResponse
        {
            Items = _mapper.Map<IEnumerable<LoanResponse>>(loans.Items),
            TotalCount = loans.TotalCount,
            TotalPages = loans.TotalPages,
            Page = loans.Page,
            PageSize = loans.PageSize
        };
    }
}
