using BookCatalog.Application.Responses.Author;

namespace BookCatalog.Application.Requests.Author.Command;

public class UpdateAuthorByIdCommand : ICommand<AuthorResponse>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
