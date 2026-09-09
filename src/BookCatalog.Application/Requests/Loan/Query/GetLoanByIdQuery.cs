using BookCatalog.Application.Responses.Loan;

namespace BookCatalog.Application.Requests.Loan.Query;

public class GetLoanByIdQuery : IQuery<LoanResponse>
{
    public Guid Id { get; set; }
}
