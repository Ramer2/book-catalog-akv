using BookCatalog.Application.Responses.Loan;

namespace BookCatalog.Application.Requests.Loan.Command;

public class ReturnLoanCommand : ICommand<LoanResponse>
{
    public Guid Id { get; set; }
}
