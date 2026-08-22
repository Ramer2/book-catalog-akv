using BookCatalog.Application.Responses.Book.Query;

namespace BookCatalog.Application.Requests.Book.Command;

public class UpdateBookByIdCommand : ICommand<BookResponse>
{
    public Guid Id { get; set; }
    public string Isbn { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int NumberOfPages { get; set; }
    public DateTime? PublishDate { get; set; }
}
