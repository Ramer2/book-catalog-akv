using BookCatalog.Application.Responses.Loan;
using BookCatalog.Domain.Pagination;

namespace BookCatalog.Application.Requests.Loan.Query;

public record GetUserLoansQuery : BaseSearchModelPagedQuery, IQuery<GetAllLoansResponse>
{
    public Guid UserId { get; set; }
    public bool? IsReturned { get; init; }
}
