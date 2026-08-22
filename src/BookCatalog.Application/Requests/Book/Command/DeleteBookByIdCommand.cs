namespace BookCatalog.Application.Requests.Book.Command;

public class DeleteBookByIdCommand : ICommand
{
    public Guid Id { get; set; }
}
