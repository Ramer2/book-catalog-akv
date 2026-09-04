using BookCatalog.Application.Responses.Author;
using BookCatalog.Domain.SearchModels;

namespace BookCatalog.Application.Requests.Author.Query;

public record GetAllAuthorsQuery : AuthorSearchModel, IQuery<GetAllAuthorsResponse>
{
}
