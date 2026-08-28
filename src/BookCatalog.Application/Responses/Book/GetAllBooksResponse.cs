namespace BookCatalog.Application.Responses.Book;

public class GetAllBooksResponse
{
    public List<BookResponse> Books { get; set; } = null!;
}