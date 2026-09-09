namespace BookCatalog.Domain.Models;

public class Loan
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public Book? Book { get; set; }
    public User? User { get; set; }

    public Loan()
    {
    }

    public Loan(Guid bookId, Guid userId)
    {
        BookId = bookId;
        UserId = userId;
        BorrowedAt = DateTime.UtcNow;
    }
}
