using BookCatalog.Application.Requests.Loan.Command;
using BookCatalog.Domain.Models;

namespace BookCatalog.Tests.TestUtils;

internal static class LoanFaker
{
    public static readonly DateTime ValidBorrowedAt =
        new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public static Loan Loan(
        Guid? id = null,
        Guid? bookId = null,
        Guid? userId = null,
        DateTime? borrowedAt = null,
        DateTime? returnedAt = null)
    {
        return new Loan
        {
            Id = id ?? Guid.NewGuid(),
            BookId = bookId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            BorrowedAt = borrowedAt ?? ValidBorrowedAt,
            ReturnedAt = returnedAt
        };
    }

    public static BorrowBookCommand BorrowCommand(
        Guid? bookId = null,
        Guid? userId = null)
    {
        return new BorrowBookCommand
        {
            BookId = bookId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid()
        };
    }
}
