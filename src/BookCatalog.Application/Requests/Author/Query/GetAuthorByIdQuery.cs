using BookCatalog.Application.Responses.Author;

namespace BookCatalog.Application.Requests.Author.Query;

public class GetAuthorByIdQuery : IQuery<AuthorResponse>
{
    public Guid Id { get; set; }
}
