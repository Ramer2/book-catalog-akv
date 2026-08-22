using BookCatalog.Application.Responses.Book.Query;

namespace BookCatalog.Application.Requests.Book.Command;

public class CreateBookCommand : ICommand<BookResponse>
{
    public string Isbn { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int NumberOfPages { get; set; }
    public DateTime? PublishDate { get; set; }
}
