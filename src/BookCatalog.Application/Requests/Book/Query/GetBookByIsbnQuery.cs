using BookCatalog.Application.Responses.Book;

namespace BookCatalog.Application.Requests.Book.Query;

public class GetBookByIsbnQuery : IQuery<BookResponse>
{
    public string Isbn { get; set; } = null!;
}
