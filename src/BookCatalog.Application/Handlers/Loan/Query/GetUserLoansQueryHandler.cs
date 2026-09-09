using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Loan.Query;
using BookCatalog.Application.Responses.Loan;
using BookCatalog.Application.Services.User;
using BookCatalog.Domain.SearchModels;
using MediatR;

namespace BookCatalog.Application.Handlers.Loan.Query;

public class GetUserLoansQueryHandler : IRequestHandler<GetUserLoansQuery, GetAllLoansResponse>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetUserLoansQueryHandler(ILoanRepository loanRepository, IUserService userService, IMapper mapper)
    {
        _loanRepository = loanRepository;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<GetAllLoansResponse> Handle(GetUserLoansQuery request, CancellationToken cancellationToken)
    {
        // Ensure the user exists first.
        await _userService.GetOrThrowAsync(request.UserId, cancellationToken);

        var searchModel = new LoanSearchModel
        {
            UserId = request.UserId,
            IsReturned = request.IsReturned,
            SortBy = request.SortBy,
            Desc = request.Desc,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var loans = await _loanRepository.GetLoansByUserIdAsync(request.UserId, searchModel, cancellationToken);
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
