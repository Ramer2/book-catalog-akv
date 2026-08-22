using BookCatalog.Application.Responses.Book.Query;

namespace BookCatalog.Application.Requests.Book.Query;

public class GetBookByIsbnQuery : IQuery<BookResponse>
{
    public string Isbn { get; set; } = null!;
}
