namespace BookCatalog.Application.Responses.Book.Query;

public class GetAllBooksResponse
{
    public List<BookResponse> Books { get; set; } = null!;
}