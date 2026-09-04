using BookCatalog.Application.Responses.Author;

namespace BookCatalog.Application.Responses.Book;

public class BookResponse
{
    public Guid Id { get; set; }
    public string Isbn { get; set; } = null!;
    public string Title { get; set; } = null!;
    public AuthorResponse Author { get; set; } = null!;
    public int NumberOfPages { get; set; }
    public DateTime? PublishDate { get; set; }
}