namespace BookCatalog.Application.Requests.Author.Command;

public class DeleteAuthorByIdCommand : ICommand
{
    public Guid Id { get; set; }
}
