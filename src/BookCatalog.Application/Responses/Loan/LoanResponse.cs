using BookCatalog.Application.Responses.Book;
using BookCatalog.Application.Responses.User;

namespace BookCatalog.Application.Responses.Loan;

public class LoanResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public BookResponse? Book { get; set; }
    public UserResponse? User { get; set; }
}
