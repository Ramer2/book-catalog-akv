using BookCatalog.Application.Responses.Loan;
using BookCatalog.Domain.SearchModels;

namespace BookCatalog.Application.Requests.Loan.Query;

public record GetAllLoansQuery : LoanSearchModel, IQuery<GetAllLoansResponse>
{
}
