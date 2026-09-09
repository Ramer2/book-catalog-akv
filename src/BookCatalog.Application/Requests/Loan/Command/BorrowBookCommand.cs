using BookCatalog.Application.Responses.Loan;

namespace BookCatalog.Application.Requests.Loan.Command;

public class BorrowBookCommand : ICommand<LoanResponse>, ITransactionalCommand
{
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
}
