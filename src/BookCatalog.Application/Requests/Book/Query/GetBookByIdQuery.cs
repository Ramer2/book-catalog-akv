using BookCatalog.Application.Responses.Book;

namespace BookCatalog.Application.Requests.Book.Query;

public class GetBookByIdQuery : IQuery<BookResponse>
{
    public Guid Id { get; set; }
}