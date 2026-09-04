namespace BookCatalog.Domain.Exceptions;

public class BookAlreadyBorrowedException : Exception
{
    public BookAlreadyBorrowedException()
    {
    }

    public BookAlreadyBorrowedException(string? message) : base(message)
    {
    }

    public BookAlreadyBorrowedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
